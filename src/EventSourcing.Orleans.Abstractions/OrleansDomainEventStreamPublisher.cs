using Flensrocker.EventSourcing.Abstractions;
using Flensrocker.EventSourcing.Abstractions.PersistentProjection;
using Microsoft.CodeAnalysis;
using Orleans.Streams;
using Polly;
using Polly.Timeout;

namespace Flensrocker.EventSourcing.Orleans.Abstractions;

internal class OrleansDomainEventStreamPublisher(
  IClusterClient clusterClient,
  OrleansDomainEventStreamPublisherMetrics metrics)
  : IDomainEventPublisher
{
  private readonly IStreamProvider _streamProvider = clusterClient.GetStreamProvider(OrleansConstants.DomainEventStreamProviderName);
  private readonly OrleansDomainEventStreamPublisherMetrics _metrics = metrics;
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

  async Task IDomainEventPublisher.InitializeAsync(CancellationToken cancellationToken)
  {
    do
    {
      try
      {
        // TODO Wait until StreamProvider is ready, this throws NullRef if called too early
        _ = _streamProvider.IsRewindable;
        break;
      }
      catch (NullReferenceException)
      {
        await Task.Delay(TimeSpan.FromSeconds(1L), cancellationToken).ConfigureAwait(false);
      }
    } while (true);
  }

  async Task IDomainEventPublisher.PublishAsync(DomainEventMetadata metadata, object payload, CancellationToken cancellationToken)
  {
    await _pipeline.ExecuteAsync(async (cancellationToken) =>
    {
      if (metadata.StreamVersion == 0)
      {
        var streamId = StreamId.Create(OrleansConstants.PublicMessageStreamNamespace, metadata.StreamType);
        var stream = _streamProvider.GetStream<object>(streamId);
        await stream.OnNextAsync(payload).ConfigureAwait(false);

        _metrics.MessagePublished();
      }
      else
      {
        var domainEvent = new DomainEvent
        {
          Metadata = metadata,
          Payload = payload,
        };

        var streamId = StreamId.Create(metadata.StreamType, metadata.StreamId);
        var stream = _streamProvider.GetStream<DomainEvent>(streamId);
        await stream.OnNextAsync(domainEvent).ConfigureAwait(false);

        _metrics.DomainEventPublished();
      }
    }, cancellationToken).ConfigureAwait(false);
  }
}
