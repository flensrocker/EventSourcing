namespace Flensrocker.EventSourcing.Abstractions;

internal class NoopDomainEventPublisher : IDomainEventPublisher
{
  public Task InitializeAsync(CancellationToken cancellationToken)
  {
    return Task.CompletedTask;
  }

  public Task PublishAsync(DomainEventMetadata metadata, object payload, CancellationToken cancellationToken)
  {
    // TODO update metrics
    return Task.CompletedTask;
  }
}
