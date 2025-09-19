using Orleans.EventSourcing;
using Orleans.EventSourcing.CustomStorage;
using OrleansPlayground.Abstractions.Domain;
using OrleansPlayground.Abstractions.EntityFrameworkCore;

namespace OrleansPlayground.Abstractions.Orleans;

public abstract class DomainEventSourcedGrain<TState, TEventBase>
  : JournaledGrain<TState, DomainEvent<TEventBase>>, ICustomStorageInterface<TState, DomainEvent<TEventBase>>
  where TState : class, IEventSourceable<TEventBase>, new()
  where TEventBase : class
{
  private readonly IDomainEventStore<Guid, TEventBase> _eventStore;
  private int _nextVersion;

  public DomainEventSourcedGrain(IDomainEventStore<Guid, TEventBase> eventStore)
  {
    _eventStore = eventStore;
  }

  public override Task OnActivateAsync(CancellationToken cancellationToken)
  {
    _nextVersion = Version + 1;
    return base.OnActivateAsync(cancellationToken);
  }

  public void RaiseDomainEvent<TEvent>(TEvent payload)
    where TEvent : TEventBase
  {
    RaiseEvent(new DomainEvent<TEventBase>
    {
      Metadata = new DomainEventMetadata
      {
        CreatedAt = DateTime.UtcNow,
        EntityType = typeof(TState).FullName ?? "",
        EntityId = this.GetPrimaryKey(),
        EntityVersion = _nextVersion,
      },
      Payload = payload,
    });

    _nextVersion++;
  }

  public Task<bool> ApplyUpdatesToStorage(IReadOnlyList<DomainEvent<TEventBase>> updates, int expectedVersion)
    => _eventStore.ApplyUpdatesToStorage(this.GetPrimaryKey(), updates, expectedVersion);

  public Task<KeyValuePair<int, TState>> ReadStateFromStorage()
    => _eventStore.ReadStateFromStorage<TState>(this.GetPrimaryKey());
}
