using OrleansPlayground.Abstractions.Domain;

namespace OrleansPlayground.Abstractions.Persistence;

public interface IDomainEventStore<TEventBase>
  where TEventBase : class
{
  Task<KeyValuePair<int, TState>> ReadStateFromStorage<TState>(Guid id)
    where TState : class, IDomainEventSourceable<TEventBase>, new();

  Task<bool> ApplyUpdatesToStorage(Guid id, IReadOnlyList<DomainEvent<TEventBase>> updates, int expectedVersion);

  Task<IReadOnlyList<DomainEvent<TEventBase>>?> ReadEventsFromStorage(Guid id, int fromVersion, int toVersionExcl);
}
