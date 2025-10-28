namespace Flensrocker.Domain.Persistence.InMemory;

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

  // Format: "Type.FullName, Assembly.Name" like AssemblyQualifiedName but without version
  internal string PayloadType { get; set; } = "";
  internal string PayloadJson { get; set; } = "";
}
