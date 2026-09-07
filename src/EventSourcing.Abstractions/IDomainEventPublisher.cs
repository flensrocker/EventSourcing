namespace Flensrocker.EventSourcing.Abstractions;

public interface IDomainEventPublisher
{
  Task InitializeAsync(CancellationToken cancellationToken);
  Task PublishAsync(
    DomainEventMetadata metadata,
    object payload,
    CancellationToken cancellationToken
  );
}
