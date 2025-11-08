using System.Collections.Concurrent;
using System.Collections.Immutable;
using Flensrocker.Domain.Abstractions;

namespace Flensrocker.Domain.Persistence.InMemory;

internal class InMemoryDomainEventStorage<TEventBase>(InMemoryDomainEventStorageOptions<TEventBase> options) : IDomainEventStorage<TEventBase>
  where TEventBase : class
{
  private static ConcurrentDictionary<Guid, ImmutableArray<DomainEventEntity>> _storage = new();

  private readonly InMemoryDomainEventStorageOptions<TEventBase> _options = options;

  Task<KeyValuePair<int, TState>> IDomainEventStorage<TEventBase>.ReadStateFromStorage<TState>(Guid streamId, CancellationToken cancellationToken)
  {
    var state = new TState();

    if (!_storage.TryGetValue(streamId, out var events))
      return Task.FromResult(new KeyValuePair<int, TState>(0, state));

    state.ApplyEvents(events.Select(ev => ev.GetDomainEvent<TEventBase>(_options.JsonOptions)));
    return Task.FromResult(new KeyValuePair<int, TState>(events.Length, state));
  }

  Task<bool> IDomainEventStorage<TEventBase>.ApplyUpdatesToStorage(Guid streamId, IReadOnlyList<DomainEvent<TEventBase>> updates, int expectedVersion)
  {
    ImmutableArray<DomainEventEntity> events;

    if (expectedVersion == 0)
      events = [];
    else if (!_storage.TryGetValue(streamId, out events) || events.Length != expectedVersion)
      return Task.FromResult(false);

    var nextEvents = events.AddRange(updates
      .Select(ev => new DomainEventEntity
      {
        Id = ev.Metadata.EventId,
        GlobalIndex = Interlocked.Increment(ref InMemoryDomainEventStorageExtensions.GlobalIndex),
      }.SetDomainEvent(ev, _options.JsonOptions)));
    var success = expectedVersion == 0
      ? _storage.TryAdd(streamId, nextEvents)
      : _storage.TryUpdate(streamId, nextEvents, events);

    return Task.FromResult(success);
  }

  Task<IReadOnlyList<DomainEvent<TEventBase>>?> IDomainEventStorage<TEventBase>.ReadEventsFromStorage(Guid streamId, int fromVersionIncl, int toVersionExcl, CancellationToken cancellationToken)
  {
    if (fromVersionIncl <= 0)
      return Task.FromResult<IReadOnlyList<DomainEvent<TEventBase>>?>(null);

    if (!_storage.TryGetValue(streamId, out var events))
      return Task.FromResult<IReadOnlyList<DomainEvent<TEventBase>>?>(null);

    if (fromVersionIncl > events.Length)
      return Task.FromResult<IReadOnlyList<DomainEvent<TEventBase>>?>(null);

    if (toVersionExcl <= 0)
      toVersionExcl = events.Length;
    else if (toVersionExcl <= fromVersionIncl || toVersionExcl > events.Length)
      return Task.FromResult<IReadOnlyList<DomainEvent<TEventBase>>?>(null);

    return Task.FromResult<IReadOnlyList<DomainEvent<TEventBase>>?>(events[(fromVersionIncl - 1)..toVersionExcl].Select(ev => ev.GetDomainEvent<TEventBase>(_options.JsonOptions)).ToImmutableArray());
  }
}
