using System.Collections.Concurrent;
using OrleansPlayground.Abstractions.Domain;
using OrleansPlayground.Abstractions.EntityFrameworkCore;

namespace OrleansPlayground.EntityFrameworkCore;

// TODO => EFCore/PostgreSQL
public class InMemoryEventStore<TId, TEventBase> : IEventStore<TId, TEventBase>
  where TId : notnull
  where TEventBase : class
{
  private ConcurrentDictionary<TId, IReadOnlyCollection<TEventBase>> _storage = new();

  public Task<KeyValuePair<int, TState>> ReadStateFromStorage<TState>(TId id)
    where TState : class, IState<TEventBase>, new()
  {
    var events = _storage.GetOrAdd(id, static _ => []);
    var state = new TState();
    state.ApplyEvents(events);
    return Task.FromResult(new KeyValuePair<int, TState>(events.Count, state));
  }

  public Task<bool> ApplyUpdatesToStorage(TId id, IReadOnlyList<TEventBase> updates, int expectedVersion)
  {
    if (!_storage.TryGetValue(id, out var events))
      return Task.FromResult(false);

    if (events.Count != expectedVersion)
      return Task.FromResult(false);

    var nextEvents = events.Concat(updates).ToArray();
    var success = _storage.TryUpdate(id, nextEvents, events);

    return Task.FromResult(success);
  }
}
