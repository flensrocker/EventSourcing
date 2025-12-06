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
  OrleansDomainEventStreamPublisherMetrics metrics,
  ILogger<OrleansDomainEventStreamPublisher> logger)
  : IDomainEventPublisher
  , ILifecycleParticipant<IClusterClientLifecycle>
{
  private readonly IServiceProvider _serviceProvider = serviceProvider;
  private readonly OrleansDomainEventStreamPublisherMetrics _metrics = metrics;
  private readonly ILogger<OrleansDomainEventStreamPublisher> _logger = logger;

  private readonly TaskCompletionSource<IStreamProvider> _initialized = new(TaskCreationOptions.RunContinuationsAsynchronously);
  // will be set before use, if not let it crash!
  private IStreamProvider _streamProvider = default!;

  private readonly ResiliencePipeline _pipeline = new ResiliencePipelineBuilder()
    .AddRetry(new()
    {
      Name = nameof(OrleansDomainEventStreamPublisher),
      ShouldHandle = new PredicateBuilder().Handle<TimeoutRejectedException>(),
      Delay = TimeSpan.FromSeconds(1),
      MaxRetryAttempts = 5,
      BackoffType = DelayBackoffType.Constant,
    })
    .AddTimeout(TimeSpan.FromSeconds(10))
    .Build();

  void ILifecycleParticipant<IClusterClientLifecycle>.Participate(IClusterClientLifecycle lifecycle)
  {
    lifecycle.Subscribe<OrleansDomainEventStreamPublisher>(
      ServiceLifecycleStage.Active,
      _ =>
      {
        var clusterClient = _serviceProvider.GetRequiredService<IClusterClient>();
        var streamProvider = clusterClient.GetStreamProvider(OrleansConstants.DomainEventStreamProviderName);
        _initialized.TrySetResult(streamProvider);
        return Task.CompletedTask;
      });
  }

  async Task IDomainEventPublisher.InitializeAsync(CancellationToken cancellationToken)
  {
    _streamProvider = await _initialized.Task.ConfigureAwait(false);
  }

  async Task IDomainEventPublisher.PublishAsync(DomainEventMetadata metadata, object payload, CancellationToken cancellationToken)
  {
    await _pipeline.ExecuteAsync(async (cancellationToken) =>
    {
      if (metadata.StreamVersion == 0)
      {
        LogPublishingPublicMessage(_logger, payload.GetType());
        var streamId = StreamId.Create(OrleansConstants.PublicMessageStreamNamespace, metadata.StreamType);
        var stream = _streamProvider.GetStream<object>(streamId);
        await stream.OnNextAsync(payload).ConfigureAwait(false);

        _metrics.MessagePublished();
        LogPublishedPublicMessage(_logger, payload.GetType());
      }
      else
      {
        LogPublishingDomainEvent(_logger, payload.GetType());

        var domainEvent = new DomainEvent
        {
          Metadata = metadata,
          Payload = payload,
        };

        var streamId = StreamId.Create(metadata.StreamType, metadata.StreamId);
        var stream = _streamProvider.GetStream<DomainEvent>(streamId);
        await stream.OnNextAsync(domainEvent).ConfigureAwait(false);

        _metrics.DomainEventPublished();
        LogPublishedDomainEvent(_logger, payload.GetType());
      }
    }, cancellationToken).ConfigureAwait(false);
  }

  [LoggerMessage(LogLevel.Information, Message = "Publishing public message {MessageType}")]
  internal static partial void LogPublishingPublicMessage(ILogger logger, Type messageType);

  [LoggerMessage(LogLevel.Information, Message = "Published public message {MessageType}")]
  internal static partial void LogPublishedPublicMessage(ILogger logger, Type messageType);

  [LoggerMessage(LogLevel.Information, Message = "Publishing domain event {DomainEventType}")]
  internal static partial void LogPublishingDomainEvent(ILogger logger, Type domainEventType);

  [LoggerMessage(LogLevel.Information, Message = "Published domain event {DomainEventType}")]
  internal static partial void LogPublishedDomainEvent(ILogger logger, Type domainEventType);
}
