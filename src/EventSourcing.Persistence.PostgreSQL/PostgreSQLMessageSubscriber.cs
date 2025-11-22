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
  IDomainEventPublisher publisher,
  PostgreSQLMessageSubscriberMetrics metrics,
  ILogger<PostgreSQLMessageSubscriber> logger)
  : BackgroundService
{
  private readonly IConfiguration _configuration = configuration;
  private readonly PostgreSQLMessageSubscriberOptions _options = options;
  private readonly IDomainEventPublisher _publisher = publisher;
  private readonly PostgreSQLMessageSubscriberMetrics _metrics = metrics;
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
      try
      {
        await pipeline.ExecuteAsync(static async (state, cancellationToken) =>
        {
          await using var conn = new LogicalReplicationConnection(state.ConnectionString);
          await conn.Open(cancellationToken).ConfigureAwait(false);

          var slot = new PgOutputReplicationSlot(Constants.PublicationSlotName);

          await foreach (var message in conn.StartReplication(slot, new PgOutputReplicationOptions(Constants.PublicationName, PgOutputProtocolVersion.V4, binary: true), cancellationToken).ConfigureAwait(false))
          {
            state.Metrics.MessageReceived();

            if (message is InsertMessage insertMessage)
            {
              state.Metrics.InsertReceived();

              var domainEvent = await insertMessage.ToDomainEventAsync(state.JsonOptions, state.Logger, cancellationToken).ConfigureAwait(false);
              if (domainEvent.HasValue)
              {
                state.Metrics.InsertSent();

                await state.Publisher.PublishAsync(domainEvent.Value.Metadata, domainEvent.Value.Payload, cancellationToken).ConfigureAwait(false);
              }
            }

            conn.SetReplicationStatus(message.WalEnd);
          }
        }, (ConnectionString: connectionString, _options.JsonOptions, Publisher: _publisher, Metrics: _metrics, Logger: _logger), stoppingToken).ConfigureAwait(false);
      }
      catch (OperationCanceledException opex) when (opex.CancellationToken == stoppingToken && stoppingToken.IsCancellationRequested)
      {
        _logger.LogInformation("Stopping PostgreSQL Message Subscriber as requested");
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Reconnecting PostgreSQL Message Subscriber");
      }
    }
  }
}
