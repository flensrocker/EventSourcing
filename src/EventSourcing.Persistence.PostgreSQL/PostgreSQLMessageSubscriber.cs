using System.Diagnostics.Metrics;
using Flensrocker.EventSourcing.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql.Replication;
using Npgsql.Replication.PgOutput;
using Npgsql.Replication.PgOutput.Messages;
using Polly;
using Polly.Retry;

namespace Flensrocker.EventSourcing.Persistence.PostgreSQL;

internal class PostgreSQLMessageSubscriber(
  IConfiguration configuration,
  PostgreSQLMessageSubscriberOptions options,
  IEnumerable<IDomainEventPublisher> publisherList,
  ILogger<PostgreSQLMessageSubscriber> logger)
  : BackgroundService
{
  private static Meter _meter = new Meter("Flensrocker.EventSourcing", "1.0.0");
  private static Counter<long> _receivedMessages = _meter.CreateCounter<long>("messages-received", "Messages", "The number of messages received from PostgreSQL");
  private static Counter<long> _receivedInserts = _meter.CreateCounter<long>("inserts-received", "Messages", "The number of insert-messages received from PostgreSQL");
  private static Counter<long> _sentInserts = _meter.CreateCounter<long>("inserts-sent", "Messages", "The number of insert-messages sent to publisher");

  private readonly IConfiguration _configuration = configuration;
  private readonly PostgreSQLMessageSubscriberOptions _options = options;
  private readonly IEnumerable<IDomainEventPublisher> _publisherList = publisherList;
  private readonly ILogger<PostgreSQLMessageSubscriber> _logger = logger;

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    var connectionString = _configuration.GetConnectionString(_options.ConnectionName);

    var pipeline = new ResiliencePipelineBuilder()
      .AddRetry(new RetryStrategyOptions
      {
        Name = nameof(PostgreSQLMessageSubscriber),
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
          _receivedMessages.Add(1L);

          if (message is InsertMessage insertMessage)
          {
            _receivedInserts.Add(1L);

            var domainEvent = await insertMessage.ToDomainEventAsync(state.JsonOptions, state.Logger, cancellationToken).ConfigureAwait(false);
            if (domainEvent.HasValue)
            {
              _sentInserts.Add(1L);

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
