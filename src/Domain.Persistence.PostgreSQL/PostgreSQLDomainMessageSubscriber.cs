using System.Collections.Immutable;
using Flensrocker.Domain.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql.Replication;
using Npgsql.Replication.PgOutput;
using Npgsql.Replication.PgOutput.Messages;

namespace Flensrocker.Domain.Persistence.PostgreSQL;

internal class PostgreSQLDomainMessageSubscriber(
  IConfiguration configuration,
  PostgreSQLDomainMessageSubscriberOptions options,
  IEnumerable<IDomainEventPublisher> publisherList,
  ILogger<PostgreSQLDomainMessageSubscriber> logger)
  : BackgroundService
{
  private readonly IConfiguration _configuration = configuration;
  private readonly PostgreSQLDomainMessageSubscriberOptions _options = options;
  private readonly ImmutableArray<IDomainEventPublisher> _publisherList = publisherList.ToImmutableArray();
  private readonly ILogger<PostgreSQLDomainMessageSubscriber> _logger = logger;

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    var connectionString = _configuration.GetConnectionString(_options.ConnectionName);

    // TODO add error handling
    await using var conn = new LogicalReplicationConnection(connectionString);
    await conn.Open(stoppingToken).ConfigureAwait(false);

    var slot = new PgOutputReplicationSlot(Constants.PublicationSlotName);

    await foreach (var message in conn.StartReplication(slot, new PgOutputReplicationOptions(Constants.PublicationName, PgOutputProtocolVersion.V4, binary: true), stoppingToken).ConfigureAwait(false))
    {
      if (message is InsertMessage insertMessage)
      {
        var domainEvent = await insertMessage.ToDomainEventAsync(_options.JsonOptions, _logger, stoppingToken).ConfigureAwait(false);
        if (domainEvent.HasValue)
        {
          _logger.LogInformation("""
            Publishing domain-event with
            {Metadata}
            {Payload}
            """, domainEvent.Value.Metadata, domainEvent.Value.Payload);

          foreach (var publisher in _publisherList)
            await publisher.PublishAsync(domainEvent.Value.Metadata, domainEvent.Value.Payload, stoppingToken).ConfigureAwait(false);
        }
      }

      conn.SetReplicationStatus(message.WalEnd);
    }
  }
}
