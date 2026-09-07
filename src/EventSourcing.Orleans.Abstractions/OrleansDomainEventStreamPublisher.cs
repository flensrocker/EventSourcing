using System.Diagnostics;
using Flensrocker.EventSourcing.Abstractions;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans.Streams;
using Polly;
using Polly.Timeout;

namespace Flensrocker.EventSourcing.Orleans.Abstractions;

internal partial class OrleansDomainEventStreamPublisher(
  IServiceProvider serviceProvider,
  EventSourcingActivitySource activitySource,
  OrleansDomainEventStreamPublisherMetrics metrics,
  ILogger<OrleansDomainEventStreamPublisher> logger
) : IDomainEventPublisher, ILifecycleParticipant<IClusterClientLifecycle>, ILifecycleParticipant<ISiloLifecycle>
{
  private readonly IServiceProvider _serviceProvider = serviceProvider;
  private readonly EventSourcingActivitySource _activitySource = activitySource;
  private readonly OrleansDomainEventStreamPublisherMetrics _metrics = metrics;
  private readonly ILogger<OrleansDomainEventStreamPublisher> _logger = logger;

  private readonly TaskCompletionSource<IStreamProvider> _initialized = new(TaskCreationOptions.RunContinuationsAsynchronously);

  // will be set before use, if not let it crash!
  private IStreamProvider _streamProvider = default!;

  private readonly ResiliencePipeline _pipeline = new ResiliencePipelineBuilder()
    .AddRetry(
      new()
      {
        Name = nameof(OrleansDomainEventStreamPublisher),
        ShouldHandle = new PredicateBuilder().Handle<TimeoutRejectedException>(),
        Delay = TimeSpan.FromSeconds(1),
        MaxRetryAttempts = 5,
        BackoffType = DelayBackoffType.Constant,
      }
    )
    .AddTimeout(TimeSpan.FromSeconds(10))
    .Build();

  private Task InitializeStreamProvider()
  {
    LogRetrievingOrleansStreamProvider(_logger);

    var clusterClient = _serviceProvider.GetRequiredService<IClusterClient>();
    var streamProvider = clusterClient.GetStreamProvider(OrleansConstants.DomainEventStreamProviderName);
    _initialized.TrySetResult(streamProvider);
    return Task.CompletedTask;
  }

  void ILifecycleParticipant<IClusterClientLifecycle>.Participate(IClusterClientLifecycle lifecycle)
  {
    LogSubscribingOrleansLifecycle(_logger, typeof(IClusterClientLifecycle));

    lifecycle.Subscribe<OrleansDomainEventStreamPublisher>(ServiceLifecycleStage.Active, _ => InitializeStreamProvider());
  }

  void ILifecycleParticipant<ISiloLifecycle>.Participate(ISiloLifecycle lifecycle)
  {
    LogSubscribingOrleansLifecycle(_logger, typeof(ISiloLifecycle));

    lifecycle.Subscribe<OrleansDomainEventStreamPublisher>(ServiceLifecycleStage.Active, _ => InitializeStreamProvider());
  }

  async Task IDomainEventPublisher.InitializeAsync(CancellationToken cancellationToken)
  {
    using var activity = _activitySource.Source.StartActivity(
      "InitializeOrleansStreamPublisher",
      ActivityKind.Internal,
      parentContext: Activity.Current?.Context ?? default
    );
    LogInitializingStreamProvider(_logger);
    _streamProvider = await _initialized.Task.ConfigureAwait(false);
    LogInitializedStreamProvider(_logger);
  }

  async Task IDomainEventPublisher.PublishAsync(DomainEventMetadata metadata, object payload, CancellationToken cancellationToken)
  {
    await _pipeline
      .ExecuteAsync(
        async (cancellationToken) =>
        {
          using var activity = _activitySource.Source.StartActivity(
            "PublishToOrleansStream",
            ActivityKind.Internal,
            parentContext: Activity.Current?.Context ?? default
          );

          if (metadata.StreamVersion == 0)
          {
            LogPublishingPublicMessage(_logger, payload.GetType());

            var startingTimestamp = Stopwatch.GetTimestamp();
            var streamId = StreamId.Create(OrleansConstants.PublicMessageStreamNamespace, metadata.StreamType);
            var stream = _streamProvider.GetStream<object>(streamId);
            await stream.OnNextAsync(payload).ConfigureAwait(false);
            _metrics.PublishToStreamDuration(startingTimestamp);

            _metrics.MessagePublished();
            LogPublishedPublicMessage(_logger, payload.GetType());
          }
          else
          {
            LogPublishingDomainEvent(_logger, payload.GetType());

            var domainEvent = new DomainEvent { Metadata = metadata, Payload = payload };

            var startingTimestamp = Stopwatch.GetTimestamp();
            var streamId = StreamId.Create(metadata.StreamType, metadata.StreamId);
            var stream = _streamProvider.GetStream<DomainEvent>(streamId);
            await stream.OnNextAsync(domainEvent).ConfigureAwait(false);
            _metrics.PublishToStreamDuration(startingTimestamp);

            _metrics.DomainEventPublished();
            LogPublishedDomainEvent(_logger, payload.GetType());
          }
        },
        cancellationToken
      )
      .ConfigureAwait(false);
  }

  [LoggerMessage(LogLevel.Information, Message = "Initializing Orleans Stream Provider")]
  internal static partial void LogInitializingStreamProvider(ILogger logger);

  [LoggerMessage(LogLevel.Information, Message = "Subscribing to Orleans lifecycle {LifecycleType}")]
  internal static partial void LogSubscribingOrleansLifecycle(ILogger logger, Type lifecycleType);

  [LoggerMessage(LogLevel.Information, Message = "Retrieving Orleans Stream Provider")]
  internal static partial void LogRetrievingOrleansStreamProvider(ILogger logger);

  [LoggerMessage(LogLevel.Information, Message = "Initialized Orleans Stream Provider")]
  internal static partial void LogInitializedStreamProvider(ILogger logger);

  [LoggerMessage(LogLevel.Information, Message = "Publishing public message {MessageType}")]
  internal static partial void LogPublishingPublicMessage(ILogger logger, Type messageType);

  [LoggerMessage(LogLevel.Information, Message = "Published public message {MessageType}")]
  internal static partial void LogPublishedPublicMessage(ILogger logger, Type messageType);

  [LoggerMessage(LogLevel.Information, Message = "Publishing domain event {DomainEventType}")]
  internal static partial void LogPublishingDomainEvent(ILogger logger, Type domainEventType);

  [LoggerMessage(LogLevel.Information, Message = "Published domain event {DomainEventType}")]
  internal static partial void LogPublishedDomainEvent(ILogger logger, Type domainEventType);
}
