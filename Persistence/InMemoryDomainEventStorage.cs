using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Text.Json;
using OrleansPlayground.Abstractions.Domain;
using OrleansPlayground.Abstractions.Persistence;

namespace OrleansPlayground.Persistence;

internal class DomainEventEntity
{
  internal required Guid Id { get; set; }
  internal required DateTimeOffset CreatedAt { get; set; }
  internal required long GlobalIndex { get; set; }
  // PostgreSQL => maybe column per field of metadata
  internal DomainEventMetadata Metadata { get; set; } = default!;
  internal string PayloadType { get; set; } = "";
  internal string PayloadJson { get; set; } = "";
}

internal static class InMemoryDomainEventStorageExtensions
{
  // PostgreSQL => Sequence
  internal static long GlobalIndex = 0L;

  internal static DomainEventEntity SetPayload<TEventBase>(this DomainEventEntity entity, TEventBase @event)
    where TEventBase : class
  {
    entity.PayloadType = @event.GetType().FullName ?? "";
    entity.PayloadJson = JsonSerializer.Serialize(@event);

    return entity;
  }

  internal static TEventBase GetPayload<TEventBase>(this DomainEventEntity entity)
    where TEventBase : class
  {
    var type = Type.GetType(entity.PayloadType);
    return (TEventBase)JsonSerializer.Deserialize(entity.PayloadJson, type!)!;
  }

  internal static DomainEventEntity SetDomainEvent<TEventBase>(this DomainEventEntity entity, DomainEvent<TEventBase> @event)
    where TEventBase : class
  {
    entity.Metadata = @event.Metadata;
    entity.SetPayload(@event.Payload);

    return entity;
  }

  internal static DomainEvent<TEventBase> GetDomainEvent<TEventBase>(this DomainEventEntity entity)
    where TEventBase : class
  {
    return new DomainEvent<TEventBase>
    {
      Metadata = entity.Metadata,
      Payload = entity.GetPayload<TEventBase>(),
    };
  }
}

// TODO => EFCore/PostgreSQL
internal class InMemoryDomainEventStorage<TEventBase>(TimeProvider timeProvider) : IDomainEventStorage<TEventBase>
  where TEventBase : class
{
  private static ConcurrentDictionary<Guid, ImmutableArray<DomainEventEntity>> _storage = new();

  private readonly TimeProvider _timeProvider = timeProvider;

  public Task<KeyValuePair<int, TState>> ReadStateFromStorage<TState>(Guid id)
    where TState : class, IDomainEventProjectable<TEventBase>, new()
  {
    var state = new TState();

    if (!_storage.TryGetValue(id, out var events))
      return Task.FromResult(new KeyValuePair<int, TState>(0, state));

    state.ApplyEvents(events.Select(ev => ev.GetDomainEvent<TEventBase>()));
    return Task.FromResult(new KeyValuePair<int, TState>(events.Length, state));
  }

  public Task<bool> ApplyUpdatesToStorage(Guid id, IReadOnlyList<DomainEvent<TEventBase>> updates, int expectedVersion)
  {
    ImmutableArray<DomainEventEntity> events;

    if (expectedVersion == 0)
      events = [];
    else if (!_storage.TryGetValue(id, out events) || events.Length != expectedVersion)
      return Task.FromResult(false);

    var nextEvents = events.AddRange(updates
      .Select(ev => new DomainEventEntity
      {
        Id = Guid.CreateVersion7(),
        CreatedAt = _timeProvider.GetUtcNow(),
        GlobalIndex = Interlocked.Increment(ref InMemoryDomainEventStorageExtensions.GlobalIndex),
      }.SetDomainEvent(ev)));
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

    return Task.FromResult<IReadOnlyList<DomainEvent<TEventBase>>?>(events[fromVersion..toVersionExcl].Select(ev => ev.GetDomainEvent<TEventBase>()).ToImmutableArray());
  }
}
