namespace Flensrocker.Domain.Abstractions;

public interface IDomainEventStorage<TEventBase>
  where TEventBase : class
{
  Task<KeyValuePair<int, TState>> ReadStateFromStorage<TState>(Guid stateId, CancellationToken cancellationToken)
    where TState : class, IDomainEventProjectable<TEventBase>, new();

  Task<bool> ApplyUpdatesToStorage(Guid stateId, IReadOnlyList<DomainEvent<TEventBase>> updates, int expectedVersion);

  Task<IReadOnlyList<DomainEvent<TEventBase>>?> ReadEventsFromStorage(Guid stateId, int fromVersion, int toVersionExcl, CancellationToken cancellationToken);
}
