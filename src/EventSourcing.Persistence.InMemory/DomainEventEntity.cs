namespace Flensrocker.EventSourcing.Persistence.InMemory;

internal class DomainEventEntity
{
  // Primary Key
  internal Guid Id { get; set; }
  // Unique
  internal long GlobalIndex { get; set; }
  // Unique (StreamType, StreamId, StreamVersion, TenantId)
  // StreamType = typeof(TEventBase).GetTypeName()
  internal string StreamType { get; set; } = "";
  internal Guid StreamId { get; set; }
  internal int StreamVersion { get; set; }
  internal Guid? TenantId { get; set; }

  internal DateTimeOffset CreatedAt { get; set; }
  internal Guid? CreatedByUserId { get; set; }

  // Full DomainEvent<TEventBase>
  internal string EventJson { get; set; } = "";
}
