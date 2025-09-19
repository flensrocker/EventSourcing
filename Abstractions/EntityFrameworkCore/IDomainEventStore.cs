using OrleansPlayground.Abstractions.Domain;

namespace OrleansPlayground.Abstractions.EntityFrameworkCore;

public interface IDomainEventStore<TId, TEventBase>
  where TId : notnull
  where TEventBase : class
{
  Task<KeyValuePair<int, TState>> ReadStateFromStorage<TState>(TId id)
    where TState : class, IEventSourceable<TEventBase>, new();
  Task<bool> ApplyUpdatesToStorage(TId id, IReadOnlyList<DomainEvent<TEventBase>> updates, int expectedVersion);
}
