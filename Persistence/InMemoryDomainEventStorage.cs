using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Text.Json;
using OrleansPlayground.Abstractions.Domain;
using OrleansPlayground.Abstractions.Persistence;

namespace OrleansPlayground.Persistence;

internal class DomainEventEntity
{
  internal required Guid Id { get; set; }
  internal required long GlobalIndex { get; set; }
  internal Guid TenantId { get; set; }
  internal DateTimeOffset CreatedAt { get; set; }
  internal Guid CreatedByUserId { get; set; }
  internal string EntityType { get; set; } = "";
  internal Guid EntityId { get; set; }
  internal int EntityVersion { get; set; }
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
    entity.Id = @event.EventId;
    entity.TenantId = @event.Metadata.TenantId;
    entity.CreatedAt = @event.Metadata.CreatedAt;
    entity.CreatedByUserId = @event.Metadata.CreatedByUserId;
    entity.EntityType = @event.Metadata.EntityType;
    entity.EntityId = @event.Metadata.EntityId;
    entity.EntityVersion = @event.Metadata.EntityVersion;
    entity.SetPayload(@event.Payload);

    return entity;
  }

  internal static DomainEvent<TEventBase> GetDomainEvent<TEventBase>(this DomainEventEntity entity)
    where TEventBase : class
  {
    return new DomainEvent<TEventBase>
    {
      EventId = entity.Id,
      Metadata = new DomainEventMetadata
      {
        TenantId = entity.TenantId,
        CreatedAt = entity.CreatedAt,
        CreatedByUserId = entity.CreatedByUserId,
        EntityType = entity.EntityType,
        EntityId = entity.EntityId,
        EntityVersion = entity.EntityVersion,
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
        Id = ev.EventId,
        GlobalIndex = Interlocked.Increment(ref InMemoryDomainEventStorageExtensions.GlobalIndex),
      }.SetDomainEvent(ev)));
    var success = expectedVersion == 0
      ? _storage.TryAdd(stateId, nextEvents)
      : _storage.TryUpdate(stateId, nextEvents, events);

    return Task.FromResult(success);
  }

  public Task<IReadOnlyList<DomainEvent<TEventBase>>?> ReadEventsFromStorage(Guid stateId, int fromVersionIncl, int toVersionExcl)
  {
    if (fromVersionIncl < 0)
      return Task.FromResult<IReadOnlyList<DomainEvent<TEventBase>>?>(null);

    if (!_storage.TryGetValue(stateId, out var events))
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
