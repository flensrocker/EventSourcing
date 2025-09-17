using OrleansPlayground.Abstractions.Domain;

namespace OrleansPlayground.Abstractions.EntityFrameworkCore;

public interface IEventStore<TId, TEventBase>
  where TId : notnull
  where TEventBase : class
{
  Task<KeyValuePair<int, TState>> ReadStateFromStorage<TState>(TId id)
    where TState : class, IState<TEventBase>, new();
  Task<bool> ApplyUpdatesToStorage(TId id, IReadOnlyList<TEventBase> updates, int expectedVersion);
}
