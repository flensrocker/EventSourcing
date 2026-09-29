namespace Flensrocker.EventSourcing.Persistence.EntityFrameworkCore;

public class MessageEntity
{
  // Primary Key
  public Guid Id { get; set; }

  // Unique, filled by a database sequence
  public long GlobalIndex { get; set; }

  // Unique (StreamType, StreamId, StreamVersion)
  // "TypeName" for the baseclass of the payload in a DomainEvent used for JSON de-/serialization
  // Must be the same within each StreamId
  // Namespace of the Orleans Stream
  public string StreamType { get; set; } = "";

  // The id of the tracked "thing"/entity/aggregate
  public Guid StreamId { get; set; }

  // if > 0 => DomainEvent
  // if == 0 => public message
  public int StreamVersion { get; set; }
  public Guid? TenantId { get; set; }

  public DateTimeOffset CreatedAt { get; set; }
  public Guid? CreatedByUserId { get; set; }

  // OpenTelemetry data associated with the creation of this message
  public byte[]? TraceId { get; set; }
  public byte[]? SpanId { get; set; }

  // Either DomainEvent.Payload as JSON
  // Or just the public message as JSON
  public string PayloadJson { get; set; } = "";
}
