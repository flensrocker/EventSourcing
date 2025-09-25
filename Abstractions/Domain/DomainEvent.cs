namespace OrleansPlayground.Abstractions.Domain;

public record DomainEventMetadata
{
  // TODO TenantId, UserId etc.
  public required DateTimeOffset CreatedAt { get; init; }
  public long GlobalIndex { get; init; }
  public required string EntityType { get; init; }
  public required Guid EntityId { get; init; }
  public required int EntityVersion { get; init; }
}

public record DomainEvent<TEventBase>
{
  public required DomainEventMetadata Metadata { get; init; }
  public required TEventBase Payload { get; init; }
}
