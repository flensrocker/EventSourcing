using OrleansPlayground.Abstractions.Domain;

namespace OrleansPlayground.Abstractions.EntityFrameworkCore;

public interface IDomainEventStore<TEventBase>
  where TEventBase : class
{
  Task<KeyValuePair<int, TState>> ReadStateFromStorage<TState>(Guid id)
    where TState : class, IEventSourceable<TEventBase>, new();

  Task<bool> ApplyUpdatesToStorage(Guid id, IReadOnlyList<DomainEvent<TEventBase>> updates, int expectedVersion);
}
