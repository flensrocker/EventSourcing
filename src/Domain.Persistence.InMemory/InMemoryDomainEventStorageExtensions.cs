using System.Text.Json;
using Flensrocker.Domain.Abstractions;

namespace Flensrocker.Domain.Persistence.InMemory;

internal static class InMemoryDomainEventStorageExtensions
{
  // PostgreSQL => Sequence
  internal static long GlobalIndex = 0L;

  internal static DomainEventEntity SetPayload<TEventBase>(this DomainEventEntity entity, TEventBase @event)
    where TEventBase : class
  {
    entity.PayloadType = @event.GetType().GetTypeName();
    entity.PayloadJson = JsonSerializer.Serialize(@event);

    return entity;
  }

  internal static TEventBase GetPayload<TEventBase>(this DomainEventEntity entity)
    where TEventBase : class
  {
    if (string.IsNullOrWhiteSpace(entity.PayloadType)
      || Type.GetType(entity.PayloadType) is not Type type
      || JsonSerializer.Deserialize(entity.PayloadJson, type) is not TEventBase payload)
      return default!;

    return payload;
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
