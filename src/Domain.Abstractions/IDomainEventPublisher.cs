namespace Flensrocker.Domain.Abstractions;

public interface IDomainEventPublisher
{
  Task PublishAsync(DomainEventMetadata metadata, object payload, CancellationToken cancellationToken);
}
