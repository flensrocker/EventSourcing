namespace OrleansPlayground.Abstractions.Domain;

public record DomainEventMetadata
{
  public required Guid TenantId { get; init; }
  public required Guid CreatedByUserId { get; init; }
  public required string EntityType { get; init; }
  public required Guid EntityId { get; init; }
  public required int EntityVersion { get; init; }
}

public record DomainEvent<TEventBase>
{
  public required DomainEventMetadata Metadata { get; init; }
  public required TEventBase Payload { get; init; }
}
