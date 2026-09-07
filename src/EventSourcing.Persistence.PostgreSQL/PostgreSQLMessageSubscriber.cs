using System.Diagnostics;
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

internal partial class PostgreSQLMessageSubscriber(
  IConfiguration configuration,
  PostgreSQLMessageSubscriberOptions options,
  EventSourcingActivitySource activitySource,
  IDomainEventPublisher publisher,
  PostgreSQLMessageSubscriberMetrics metrics,
  ILogger<PostgreSQLMessageSubscriber> logger
) : BackgroundService
{
  private readonly IConfiguration _configuration = configuration;
  private readonly PostgreSQLMessageSubscriberOptions _options = options;
  private readonly EventSourcingActivitySource _activitySource = activitySource;
  private readonly IDomainEventPublisher _publisher = publisher;
  private readonly PostgreSQLMessageSubscriberMetrics _metrics = metrics;
  private readonly ILogger<PostgreSQLMessageSubscriber> _logger = logger;

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    using (var activity = _activitySource.Source.StartActivity("InitializeMessageSubscriber"))
    {
      LogStartingSubscriber(_logger);
      await _publisher.InitializeAsync(stoppingToken).ConfigureAwait(false);
    }

    var connectionString = _configuration.GetConnectionString(_options.ConnectionName);

    var pipeline = new ResiliencePipelineBuilder()
      .AddRetry(
        new RetryStrategyOptions
        {
          Name = nameof(PostgreSQLMessageSubscriber),
          Delay = TimeSpan.FromSeconds(2),
          MaxRetryAttempts = int.MaxValue,
          BackoffType = DelayBackoffType.Exponential,
          MaxDelay = TimeSpan.FromMinutes(1),
          UseJitter = true,
        }
      )
      .Build();

    while (stoppingToken.IsCancellationRequested is false)
    {
      try
      {
        await pipeline
          .ExecuteAsync(
            static async (state, cancellationToken) =>
            {
              LogConnecting(state.Logger);
              await using var conn = new LogicalReplicationConnection(state.ConnectionString);
              await conn.Open(cancellationToken).ConfigureAwait(false);

              var slot = new PgOutputReplicationSlot(Constants.PublicationSlotName);

              await foreach (
                var message in conn.StartReplication(
                    slot,
                    new PgOutputReplicationOptions(
                      Constants.PublicationName,
                      PgOutputProtocolVersion.V4,
                      binary: true
                    ),
                    cancellationToken
                  )
                  .ConfigureAwait(false)
              )
              {
                using var activity = state.ActivitySource.StartActivity("PublishMessage");

                LogReceivedReplicationMessage(state.Logger, message.GetType());
                state.Metrics.MessageReceived();

                if (message is InsertMessage insertMessage)
                {
                  state.Metrics.InsertReceived();
                  var startingTimestamp = Stopwatch.GetTimestamp();

                  var domainEvent = await insertMessage
                    .ToDomainEventAsync(state.JsonOptions, state.Logger, cancellationToken)
                    .ConfigureAwait(false);
                  if (domainEvent.HasValue)
                  {
                    LogPublishingInsertMessage(state.Logger, domainEvent.Value.Payload.GetType());
                    state.Metrics.InsertSent();

                    if (
                      activity is not null
                      && domainEvent.Value.Metadata.TraceId is byte[] traceId
                      && domainEvent.Value.Metadata.SpanId is byte[] spanId
                    )
                      activity.AddLink(
                        new ActivityLink(
                          new ActivityContext(
                            ActivityTraceId.CreateFromBytes(traceId),
                            ActivitySpanId.CreateFromBytes(spanId),
                            ActivityTraceFlags.None
                          )
                        )
                      );

                    await state
                      .Publisher.PublishAsync(
                        domainEvent.Value.Metadata,
                        domainEvent.Value.Payload,
                        cancellationToken
                      )
                      .ConfigureAwait(false);

                    state.Metrics.ProcessInsertMessageDuration(startingTimestamp);
                    LogPublishedInsertMessage(state.Logger, domainEvent.Value.Payload.GetType());
                  }
                }

                conn.SetReplicationStatus(message.WalEnd);
                LogAcknowledgedReplicationMessage(state.Logger);
              }
            },
            (
              ActivitySource: _activitySource.Source,
              ConnectionString: connectionString,
              _options.JsonOptions,
              Publisher: _publisher,
              Metrics: _metrics,
              Logger: _logger
            ),
            stoppingToken
          )
          .ConfigureAwait(false);
      }
      catch (OperationCanceledException opex)
        when (opex.CancellationToken == stoppingToken && stoppingToken.IsCancellationRequested)
      {
        LogStoppingSubscriber(_logger);
      }
      catch (Exception ex)
      {
        LogReconnectingSubscriber(_logger, ex);
      }
    }
  }

  [LoggerMessage(LogLevel.Information, Message = "Connecting to PostgreSQL")]
  internal static partial void LogConnecting(ILogger logger);

  [LoggerMessage(
    LogLevel.Information,
    Message = "Received replication message {ReplicationMessageType}"
  )]
  internal static partial void LogReceivedReplicationMessage(
    ILogger logger,
    Type replicationMessageType
  );

  [LoggerMessage(LogLevel.Information, Message = "Publishing insert message {InsertMessageType}")]
  internal static partial void LogPublishingInsertMessage(ILogger logger, Type insertMessageType);

  [LoggerMessage(LogLevel.Information, Message = "Published insert message {InsertMessageType}")]
  internal static partial void LogPublishedInsertMessage(ILogger logger, Type insertMessageType);

  [LoggerMessage(LogLevel.Information, Message = "Acknowledged replication message")]
  internal static partial void LogAcknowledgedReplicationMessage(ILogger logger);

  [LoggerMessage(LogLevel.Information, Message = "Starting PostgreSQL Message Subscriber")]
  internal static partial void LogStartingSubscriber(ILogger logger);

  [LoggerMessage(
    LogLevel.Information,
    Message = "Stopping PostgreSQL Message Subscriber as requested"
  )]
  internal static partial void LogStoppingSubscriber(ILogger logger);

  [LoggerMessage(LogLevel.Error, Message = "Reconnecting PostgreSQL Message Subscriber")]
  internal static partial void LogReconnectingSubscriber(ILogger logger, Exception ex);
}
