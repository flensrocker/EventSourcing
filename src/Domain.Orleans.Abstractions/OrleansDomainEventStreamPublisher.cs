using Flensrocker.Domain.Abstractions;

namespace Flensrocker.Domain.Orleans.Abstractions;

public class OrleansDomainEventStreamPublisher(IClusterClient clusterClient) : IDomainEventPublisher
{
  private readonly IClusterClient _clusterClient = clusterClient;

  async Task IDomainEventPublisher.PublishAsync(DomainEventMetadata metadata, object payload, CancellationToken cancellationToken)
  {
    var streamProvider = _clusterClient.GetStreamProvider(DomainConstants.DomainEventStreamProviderName);

    if (metadata.StreamVersion == 0)
    {
      var streamId = StreamId.Create(DomainConstants.PublicMessageStreamNamespace, metadata.StreamType);
      var stream = streamProvider.GetStream<object>(streamId);
      await stream.OnNextAsync(payload);
    }
    else
    {
      var domainEvent = new DomainEvent
      {
        Metadata = metadata,
        Payload = payload,
      };

      var streamId = StreamId.Create(metadata.StreamType, metadata.StreamId);
      var stream = streamProvider.GetStream<DomainEvent>(streamId);
      await stream.OnNextAsync(domainEvent);
    }
  }
}
