using System.Collections.Concurrent;
using OrleansPlayground.Abstractions.Domain;
using OrleansPlayground.Abstractions.EntityFrameworkCore;

namespace OrleansPlayground.EntityFrameworkCore;

// TODO => EFCore/PostgreSQL
public class InMemoryDomainEventStore<TEventBase> : IDomainEventStore<TEventBase>
  where TEventBase : class
{
  private static ConcurrentDictionary<Guid, IReadOnlyCollection<DomainEvent<TEventBase>>> _storage = new();

  public Task<KeyValuePair<int, TState>> ReadStateFromStorage<TState>(Guid id)
    where TState : class, IEventSourceable<TEventBase>, new()
  {
    var state = new TState();

    if (!_storage.TryGetValue(id, out var events))
      return Task.FromResult(new KeyValuePair<int, TState>(0, state));

    state.ApplyEvents(events);
    return Task.FromResult(new KeyValuePair<int, TState>(events.Count, state));
  }

  public Task<bool> ApplyUpdatesToStorage(Guid id, IReadOnlyList<DomainEvent<TEventBase>> updates, int expectedVersion)
  {
    IReadOnlyCollection<DomainEvent<TEventBase>>? events;

    if (expectedVersion == 0)
      events = [];
    else if (!_storage.TryGetValue(id, out events) || events.Count != expectedVersion)
      return Task.FromResult(false);

    var nextEvents = events.Concat(updates).ToArray();
    var success = expectedVersion == 0
      ? _storage.TryAdd(id, nextEvents)
      : _storage.TryUpdate(id, nextEvents, events);

    return Task.FromResult(success);
  }
}
