using Orleans.EventSourcing;
using Orleans.EventSourcing.CustomStorage;
using OrleansPlayground.Abstractions.Domain;
using OrleansPlayground.Abstractions.Persistence;

namespace OrleansPlayground.Abstractions.Orleans;

public abstract class DomainAggregateGrain<TState, TEventBase>
  : JournaledGrain<TState, DomainEvent<TEventBase>>, ICustomStorageInterface<TState, DomainEvent<TEventBase>>
  where TState : class, IDomainEventProjectable<TEventBase>, new()
  where TEventBase : class
{
  private readonly IDomainEventStorage<TEventBase> _eventStore;
  private int _nextVersion;

  protected DomainAggregateGrain()
  {
    _eventStore = ServiceProvider.GetRequiredService<IDomainEventStorage<TEventBase>>();
  }

  public override Task OnActivateAsync(CancellationToken cancellationToken)
  {
    _nextVersion = Version + 1;
    return base.OnActivateAsync(cancellationToken);
  }

  protected DomainEvent<TEventBase> RaiseDomainEvent<TEvent>(TEvent payload)
    where TEvent : TEventBase
  {
    var domainEvent = new DomainEvent<TEventBase>
    {
      Metadata = new DomainEventMetadata
      {
        EntityType = typeof(TState).FullName ?? "",
        EntityId = this.GetPrimaryKey(),
        EntityVersion = _nextVersion,
      },
      Payload = payload,
    };
    _nextVersion++;

    RaiseEvent(domainEvent);

    return domainEvent;
  }

  public Task<bool> ApplyUpdatesToStorage(IReadOnlyList<DomainEvent<TEventBase>> updates, int expectedVersion)
    => _eventStore.ApplyUpdatesToStorage(this.GetPrimaryKey(), updates, expectedVersion);

  public Task<KeyValuePair<int, TState>> ReadStateFromStorage()
    => _eventStore.ReadStateFromStorage<TState>(this.GetPrimaryKey());
}
