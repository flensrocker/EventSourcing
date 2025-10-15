using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Text.Json;
using OrleansPlayground.Abstractions.Domain;
using OrleansPlayground.Abstractions.Persistence;

namespace OrleansPlayground.Persistence;

internal class DomainEventEntity
{
  // Primary Key
  internal Guid Id { get; set; }
  // Unique
  internal long GlobalIndex { get; set; }
  // Unique (StreamType, StreamId, StreamVersion, TenantId)
  internal string StreamType { get; set; } = "";
  internal Guid StreamId { get; set; }
  internal int StreamVersion { get; set; }
  internal Guid TenantId { get; set; }

  internal DateTimeOffset CreatedAt { get; set; }
  internal Guid CreatedByUserId { get; set; }

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
    entity.PayloadType = @event?.GetType()?.FullName ?? "";
    entity.PayloadJson = @event is null ? "" : JsonSerializer.Serialize(@event);

    return entity;
  }

  internal static TEventBase GetPayload<TEventBase>(this DomainEventEntity entity)
    where TEventBase : class
  {
    return !string.IsNullOrWhiteSpace(entity.PayloadType)
      && Type.GetType(entity.PayloadType) is Type type
      && JsonSerializer.Deserialize(entity.PayloadJson, type) is TEventBase payload
      ? payload
      : default!;
  }

  internal static DomainEventEntity SetDomainEvent<TEventBase>(this DomainEventEntity entity, DomainEvent<TEventBase> @event)
    where TEventBase : class
  {
    entity.Id = @event.Metadata.EventId;
    entity.StreamType = @event.Metadata.StreamType;
    entity.StreamId = @event.Metadata.StreamId;
    entity.StreamVersion = @event.Metadata.StreamVersion;
    entity.TenantId = @event.Metadata.TenantId;
    entity.CreatedAt = @event.Metadata.CreatedAt;
    entity.CreatedByUserId = @event.Metadata.CreatedByUserId;
    entity.SetPayload(@event.Payload);

    return entity;
  }

  internal static DomainEvent<TEventBase> GetDomainEvent<TEventBase>(this DomainEventEntity entity)
    where TEventBase : class
  {
    return new DomainEvent<TEventBase>
    {
      Metadata = new DomainEventMetadata
      {
        EventId = entity.Id,
        StreamType = entity.StreamType,
        StreamId = entity.StreamId,
        StreamVersion = entity.StreamVersion,
        TenantId = entity.TenantId,
        CreatedAt = entity.CreatedAt,
        CreatedByUserId = entity.CreatedByUserId,
      },
      Payload = entity.GetPayload<TEventBase>(),
    };
  }
}

// TODO => EFCore/PostgreSQL
internal class InMemoryDomainEventStorage<TEventBase> : IDomainEventStorage<TEventBase>
  where TEventBase : class
{
  private static ConcurrentDictionary<Guid, ImmutableArray<DomainEventEntity>> _storage = new();

  public Task<KeyValuePair<int, TState>> ReadStateFromStorage<TState>(Guid stateId)
    where TState : class, IDomainEventProjectable<TEventBase>, new()
  {
    var state = new TState();

    if (!_storage.TryGetValue(stateId, out var events))
      return Task.FromResult(new KeyValuePair<int, TState>(0, state));

    state.ApplyEvents(events.Select(ev => ev.GetDomainEvent<TEventBase>()));
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
      }.SetDomainEvent(ev)));
    var success = expectedVersion == 0
      ? _storage.TryAdd(stateId, nextEvents)
      : _storage.TryUpdate(stateId, nextEvents, events);

    return Task.FromResult(success);
  }

  public Task<IReadOnlyList<DomainEvent<TEventBase>>?> ReadEventsFromStorage(Guid streamId, int fromVersionIncl, int toVersionExcl)
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

    return Task.FromResult<IReadOnlyList<DomainEvent<TEventBase>>?>(events[fromVersionIncl..toVersionExcl].Select(ev => ev.GetDomainEvent<TEventBase>()).ToImmutableArray());
  }
}
