using Flensrocker.Domain.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql.Replication;
using Npgsql.Replication.PgOutput;
using Npgsql.Replication.PgOutput.Messages;
using Polly;
using Polly.Retry;

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
  private readonly IEnumerable<IDomainEventPublisher> _publisherList = publisherList;
  private readonly ILogger<PostgreSQLDomainMessageSubscriber> _logger = logger;

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    var connectionString = _configuration.GetConnectionString(_options.ConnectionName);

    var pipeline = new ResiliencePipelineBuilder()
      .AddRetry(new RetryStrategyOptions
      {
        Name = nameof(PostgreSQLDomainMessageSubscriber),
        Delay = TimeSpan.FromSeconds(2),
        MaxRetryAttempts = int.MaxValue,
        BackoffType = DelayBackoffType.Exponential,
        MaxDelay = TimeSpan.FromMinutes(1),
        UseJitter = true,
      })
      .Build();

    while (stoppingToken.IsCancellationRequested is false)
    {
      await pipeline.ExecuteAsync(static async (state, cancellationToken) =>
      {
        await using var conn = new LogicalReplicationConnection(state.ConnectionString);
        await conn.Open(cancellationToken).ConfigureAwait(false);

        var slot = new PgOutputReplicationSlot(Constants.PublicationSlotName);

        await foreach (var message in conn.StartReplication(slot, new PgOutputReplicationOptions(Constants.PublicationName, PgOutputProtocolVersion.V4, binary: true), cancellationToken).ConfigureAwait(false))
        {
          if (message is InsertMessage insertMessage)
          {
            var domainEvent = await insertMessage.ToDomainEventAsync(state.JsonOptions, state.Logger, cancellationToken).ConfigureAwait(false);
            if (domainEvent.HasValue)
            {
              state.Logger.LogInformation("""
                Publishing domain-event with
                {Metadata}
                {Payload}
                """, domainEvent.Value.Metadata, domainEvent.Value.Payload);

              foreach (var publisher in state.PublisherList)
                await publisher.PublishAsync(domainEvent.Value.Metadata, domainEvent.Value.Payload, cancellationToken).ConfigureAwait(false);
            }
          }

          conn.SetReplicationStatus(message.WalEnd);
        }
      }, (ConnectionString: connectionString, _options.JsonOptions, PublisherList: _publisherList, Logger: _logger), stoppingToken).ConfigureAwait(false);
    }
  }
}
