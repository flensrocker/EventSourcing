namespace Flensrocker.EventSourcing.Abstractions;

public sealed record DomainEventMetadata
{
  public required Guid EventId { get; init; }
  public required string StreamType { get; init; }
  public required Guid StreamId { get; init; }
  public required int StreamVersion { get; init; }
  public required Guid? TenantId { get; init; }
  public required DateTimeOffset CreatedAt { get; init; }
  public required Guid? CreatedByUserId { get; init; }
}

public record DomainEvent
{
  public required DomainEventMetadata Metadata { get; init; }
  public required object Payload { get; init; }

  /// <summary>
  /// DRAFT
  /// Used to attach "public messages" for external services to domain events.
  /// All must be stored in the same transaction.
  /// Public messages are not applied to event-sourced aggregates/projections.
  /// </summary>
  public sealed record WithPublicMessages : DomainEvent
  {
    public required IReadOnlyCollection<object> MessageList { get; init; }
  }
}
