using Orleans.EventSourcing;
using Orleans.EventSourcing.CustomStorage;
using OrleansPlayground.Abstractions.Domain;
using OrleansPlayground.Abstractions.EntityFrameworkCore;

namespace OrleansPlayground.Abstractions.Orleans;

public abstract class DomainEventSourcedGrain<TState, TEventBase>
  : JournaledGrain<TState, DomainEvent<TEventBase>>, ICustomStorageInterface<TState, DomainEvent<TEventBase>>
  where TState : class, IDomainEventSourceable<TEventBase>, new()
  where TEventBase : class
{
  private readonly TimeProvider _timeProvider;
  private readonly IDomainEventStore<TEventBase> _eventStore;
  private int _nextVersion;

  public DomainEventSourcedGrain(TimeProvider timeProvider, IDomainEventStore<TEventBase> eventStore)
  {
    _timeProvider = timeProvider;
    _eventStore = eventStore;
  }

  public override Task OnActivateAsync(CancellationToken cancellationToken)
  {
    _nextVersion = Version + 1;
    return base.OnActivateAsync(cancellationToken);
  }

  public DomainEvent<TEventBase> RaiseDomainEvent<TEvent>(TEvent payload)
    where TEvent : TEventBase
  {
    var domainEvent = new DomainEvent<TEventBase>
    {
      Metadata = new DomainEventMetadata
      {
        CreatedAt = _timeProvider.GetUtcNow(),
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
