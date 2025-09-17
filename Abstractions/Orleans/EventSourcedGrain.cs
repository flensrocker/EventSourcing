using Orleans.EventSourcing;
using Orleans.EventSourcing.CustomStorage;
using OrleansPlayground.Abstractions.Domain;
using OrleansPlayground.Abstractions.EntityFrameworkCore;

namespace OrleansPlayground.Abstractions.Orleans;

public abstract class EventSourcedGrain<TState, TEventBase> : JournaledGrain<TState, TEventBase>, ICustomStorageInterface<TState, TEventBase>
  where TState : class, IState<TEventBase>, new()
  where TEventBase : class
{
  private readonly IEventStore<Guid, TEventBase> _eventStore;

  public EventSourcedGrain(IEventStore<Guid, TEventBase> eventStore)
  {
    _eventStore = eventStore;
  }

  public Task<bool> ApplyUpdatesToStorage(IReadOnlyList<TEventBase> updates, int expectedVersion)
    => _eventStore.ApplyUpdatesToStorage(this.GetPrimaryKey(), updates, expectedVersion);

  public Task<KeyValuePair<int, TState>> ReadStateFromStorage()
    => _eventStore.ReadStateFromStorage<TState>(this.GetPrimaryKey());
}
