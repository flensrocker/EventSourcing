namespace Flensrocker.EventSourcing.Abstractions;

public interface IDomainEventStorage<TEventBase>
  where TEventBase : class
{
  /// <returns>If the state does not exist, the result of the parameterless constructor is returned.</returns>
  Task<KeyValuePair<int, TState>> ReadStateFromStorage<TState>(Guid streamId, CancellationToken cancellationToken)
    where TState : class, IDomainEventProjectable, new();

  /// <param name="expectedVersion">If the state is expected to not exist, the expected version is 0</param>
  Task<bool> ApplyUpdatesToStorage(Guid streamId, IReadOnlyList<DomainEvent> updates, int expectedVersion);

  /// <param name="fromVersionIncl">Must be positive, the first event has a version equal to 1</param>
  /// <param name="toVersionExcl">A value of 0 means all events up to the last, otherwise must be greater than <c>fromVersionIncl</c></param>
  /// <returns><c>null</c> if the range is invalid or referencing non existing events</returns>
  Task<IReadOnlyList<DomainEvent>?> ReadEventsFromStorage(Guid streamId, int fromVersionIncl, int toVersionExcl, CancellationToken cancellationToken);

  IAsyncEnumerable<DomainEvent> ReadAllEventsFromStorage(CancellationToken cancellationToken);
}
