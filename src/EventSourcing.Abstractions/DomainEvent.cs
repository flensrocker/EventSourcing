namespace Flensrocker.EventSourcing.Abstractions;

[GenerateSerializer]
public sealed record DomainEventMetadata
{
  [Id(0)]
  public required Guid EventId { get; init; }
  [Id(1)]
  public required string StreamType { get; init; }
  [Id(2)]
  public required Guid StreamId { get; init; }
  [Id(3)]
  public required int StreamVersion { get; init; }
  [Id(4)]
  public required Guid? TenantId { get; init; }
  [Id(5)]
  public required DateTimeOffset CreatedAt { get; init; }
  [Id(6)]
  public required Guid? CreatedByUserId { get; init; }
}

[GenerateSerializer]
public record DomainEvent
{
  [Id(0)]
  public required DomainEventMetadata Metadata { get; init; }
  [Id(1)]
  public required object Payload { get; init; }

  /// <summary>
  /// DRAFT
  /// Used to attach "public messages" for external services to domain events.
  /// All must be stored in the same transaction.
  /// Public messages are not applied to event-sourced aggregates/projections.
  /// </summary>
  [GenerateSerializer]
  public sealed record WithPublicMessages : DomainEvent
  {
    [Id(0)]
    public required IReadOnlyCollection<object> MessageList { get; init; }
  }
}
