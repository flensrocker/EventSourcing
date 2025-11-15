namespace Flensrocker.EventSourcing.Abstractions;

public interface IDomainEventPublisher
{
  Task PublishAsync(DomainEventMetadata metadata, object payload, CancellationToken cancellationToken);
}
