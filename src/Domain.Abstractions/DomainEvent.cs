namespace Flensrocker.Domain.Abstractions;

public record DomainEventMetadata
{
  public required Guid EventId { get; init; }
  public required string StreamType { get; init; }
  public required Guid StreamId { get; init; }
  public required int StreamVersion { get; init; }
  public required Guid? TenantId { get; init; }
  public required DateTimeOffset CreatedAt { get; init; }
  public required Guid? CreatedByUserId { get; init; }
}

public record DomainEvent<TEventBase>
{
  public required DomainEventMetadata Metadata { get; init; }
  public required TEventBase Payload { get; init; }
}
