using System.Collections.Concurrent;
using System.Collections.Immutable;
using Flensrocker.Domain.Abstractions;

namespace Flensrocker.Domain.Persistence.InMemory;

// TODO => EFCore/PostgreSQL
public class InMemoryDomainEventStorage<TEventBase>(InMemoryDomainEventStorageOptions<TEventBase> options) : IDomainEventStorage<TEventBase>
  where TEventBase : class
{
  private static ConcurrentDictionary<Guid, ImmutableArray<DomainEventEntity>> _storage = new();

  private readonly InMemoryDomainEventStorageOptions<TEventBase> _options = options;

  public Task<KeyValuePair<int, TState>> ReadStateFromStorage<TState>(Guid stateId, CancellationToken cancellationToken)
    where TState : class, IDomainEventProjectable<TEventBase>, new()
  {
    var state = new TState();

    if (!_storage.TryGetValue(stateId, out var events))
      return Task.FromResult(new KeyValuePair<int, TState>(0, state));

    state.ApplyEvents(events.Select(ev => ev.GetDomainEvent<TEventBase>(_options.JsonOptions)));
    return Task.FromResult(new KeyValuePair<int, TState>(events.Length, state));
  }

  public Task<bool> ApplyUpdatesToStorage(Guid stateId, IReadOnlyList<DomainEvent<TEventBase>> updates, int expectedVersion)
  {
    ImmutableArray<DomainEventEntity> events;

    if (expectedVersion == 0)
      events = [];
    else if (!_storage.TryGetValue(stateId, out events) || events.Length != expectedVersion)
      return Task.FromResult(false);

    var nextEvents = events.AddRange(updates
      .Select(ev => new DomainEventEntity
      {
        Id = ev.Metadata.EventId,
        GlobalIndex = Interlocked.Increment(ref InMemoryDomainEventStorageExtensions.GlobalIndex),
      }.SetDomainEvent(ev, _options.JsonOptions)));
    var success = expectedVersion == 0
      ? _storage.TryAdd(stateId, nextEvents)
      : _storage.TryUpdate(stateId, nextEvents, events);

    return Task.FromResult(success);
  }

  public Task<IReadOnlyList<DomainEvent<TEventBase>>?> ReadEventsFromStorage(Guid streamId, int fromVersionIncl, int toVersionExcl, CancellationToken cancellationToken)
  {
    if (fromVersionIncl < 0)
      return Task.FromResult<IReadOnlyList<DomainEvent<TEventBase>>?>(null);

    if (!_storage.TryGetValue(streamId, out var events))
      return Task.FromResult<IReadOnlyList<DomainEvent<TEventBase>>?>(null);

    if (fromVersionIncl >= events.Length)
      return Task.FromResult<IReadOnlyList<DomainEvent<TEventBase>>?>(null);

    if (toVersionExcl <= 0)
      toVersionExcl = events.Length;
    else if (toVersionExcl > events.Length)
      return Task.FromResult<IReadOnlyList<DomainEvent<TEventBase>>?>(null);

    return Task.FromResult<IReadOnlyList<DomainEvent<TEventBase>>?>(events[fromVersionIncl..toVersionExcl].Select(ev => ev.GetDomainEvent<TEventBase>(_options.JsonOptions)).ToImmutableArray());
  }
}
