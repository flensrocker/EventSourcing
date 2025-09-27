using System.Collections.Concurrent;
using System.Collections.Immutable;
using OrleansPlayground.Abstractions.Domain;
using OrleansPlayground.Abstractions.Persistence;

namespace OrleansPlayground.Persistence;

internal static class GlobalInMemoryDomainEventStoreState
{
  // PostgreSQL => Sequence
  internal static long GlobalIndex = 0L;
}

internal class DomainEventEntity<TEventBase>
  where TEventBase : class
{
  internal required Guid Id { get; set; }
  internal required DateTimeOffset CreatedAt { get; set; }
  internal required long GlobalIndex { get; set; }
  // PostgreSQL => columns for payload discriminator as string and payload data as JSON
  internal required DomainEvent<TEventBase> Event { get; set; }
}

// TODO => EFCore/PostgreSQL
internal class InMemoryDomainEventStore<TEventBase>(TimeProvider timeProvider) : IDomainEventStore<TEventBase>
  where TEventBase : class
{
  private static ConcurrentDictionary<Guid, ImmutableArray<DomainEventEntity<TEventBase>>> _storage = new();

  private readonly TimeProvider _timeProvider = timeProvider;

  public Task<KeyValuePair<int, TState>> ReadStateFromStorage<TState>(Guid id)
    where TState : class, IDomainEventSourceable<TEventBase>, new()
  {
    var state = new TState();

    if (!_storage.TryGetValue(id, out var events))
      return Task.FromResult(new KeyValuePair<int, TState>(0, state));

    state.ApplyEvents(events.Select(ev => ev.Event));
    return Task.FromResult(new KeyValuePair<int, TState>(events.Length, state));
  }

  public Task<bool> ApplyUpdatesToStorage(Guid id, IReadOnlyList<DomainEvent<TEventBase>> updates, int expectedVersion)
  {
    ImmutableArray<DomainEventEntity<TEventBase>> events;

    if (expectedVersion == 0)
      events = [];
    else if (!_storage.TryGetValue(id, out events) || events.Length != expectedVersion)
      return Task.FromResult(false);

    var nextEvents = events.AddRange(updates
      .Select(ev => new DomainEventEntity<TEventBase>
      {
        Id = Guid.CreateVersion7(),
        CreatedAt = _timeProvider.GetUtcNow(),
        GlobalIndex = Interlocked.Increment(ref GlobalInMemoryDomainEventStoreState.GlobalIndex),
        Event = ev,
      }));
    var success = expectedVersion == 0
      ? _storage.TryAdd(id, nextEvents)
      : _storage.TryUpdate(id, nextEvents, events);

    return Task.FromResult(success);
  }

  public Task<IReadOnlyList<DomainEvent<TEventBase>>?> ReadEventsFromStorage(Guid id, int fromVersion, int toVersionExcl)
  {
    if (fromVersion < 0)
      return Task.FromResult<IReadOnlyList<DomainEvent<TEventBase>>?>(null);

    if (!_storage.TryGetValue(id, out var events))
      return Task.FromResult<IReadOnlyList<DomainEvent<TEventBase>>?>(null);

    if (fromVersion > events.Length)
      return Task.FromResult<IReadOnlyList<DomainEvent<TEventBase>>?>(null);

    if (toVersionExcl <= 0)
      toVersionExcl = events.Length;
    else if (toVersionExcl >= events.Length)
      return Task.FromResult<IReadOnlyList<DomainEvent<TEventBase>>?>(null);

    return Task.FromResult<IReadOnlyList<DomainEvent<TEventBase>>?>(events[fromVersion..toVersionExcl].Select(ev => ev.Event).ToImmutableArray());
  }
}
