using System.Collections.Concurrent;
using System.Collections.Immutable;
using OrleansPlayground.Abstractions.Domain;
using OrleansPlayground.Abstractions.EntityFrameworkCore;

namespace OrleansPlayground.EntityFrameworkCore;

internal static class GlobalInMemoryDomainEventStoreState
{
  // PostgreSQL => Sequence
  internal static long GlobalIndex = 0L;
}

// TODO => EFCore/PostgreSQL
internal class InMemoryDomainEventStore<TEventBase> : IDomainEventStore<TEventBase>
  where TEventBase : class
{
  private static ConcurrentDictionary<Guid, ImmutableArray<DomainEvent<TEventBase>>> _storage = new();

  public Task<KeyValuePair<int, TState>> ReadStateFromStorage<TState>(Guid id)
    where TState : class, IDomainEventSourceable<TEventBase>, new()
  {
    var state = new TState();

    if (!_storage.TryGetValue(id, out var events))
      return Task.FromResult(new KeyValuePair<int, TState>(0, state));

    state.ApplyEvents(events);
    return Task.FromResult(new KeyValuePair<int, TState>(events.Length, state));
  }

  public Task<bool> ApplyUpdatesToStorage(Guid id, IReadOnlyList<DomainEvent<TEventBase>> updates, int expectedVersion)
  {
    ImmutableArray<DomainEvent<TEventBase>> events;

    if (expectedVersion == 0)
      events = [];
    else if (!_storage.TryGetValue(id, out events) || events.Length != expectedVersion)
      return Task.FromResult(false);

    var nextEvents = events.AddRange(updates
      .Select(ev => ev with
      {
        Metadata = ev.Metadata with
        {
          GlobalIndex = Interlocked.Increment(ref GlobalInMemoryDomainEventStoreState.GlobalIndex),
        }
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

    return Task.FromResult<IReadOnlyList<DomainEvent<TEventBase>>?>(events[fromVersion..toVersionExcl]);
  }
}
