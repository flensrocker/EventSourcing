using System.Diagnostics;

namespace Flensrocker.EventSourcing.Abstractions;

[GenerateSerializer]
[Alias("Flensrocker.EventSourcing.DomainEventMetadata")]
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
  [Id(7)]
  public byte[]? TraceId { get; init; }
  [Id(8)]
  public byte[]? SpanId { get; init; }
}

[GenerateSerializer]
[Alias("Flensrocker.EventSourcing.DomainEvent")]
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
  /// Public messages are not applied to event-sourced projections.
  /// </summary>
  [GenerateSerializer]
  [Alias("Flensrocker.EventSourcing.DomainEvent.WithPublicMessages")]
  public sealed record WithPublicMessages : DomainEvent
  {
    [Id(0)]
    public required IReadOnlyCollection<object> MessageList { get; init; }
  }

  public static DomainEvent Create<TEventBase, TPayload>(Guid streamId, int streamVersion, TPayload payload, Guid? tenantId, Guid? userId, DateTimeOffset createdAt)
    where TPayload : notnull, TEventBase
  {
    byte[]? traceId = null;
    byte[]? spanId = null;
    if (Activity.Current is not null && Activity.Current.IdFormat == ActivityIdFormat.W3C)
    {
      traceId = new byte[16];
      spanId = new byte[8];
      Activity.Current.TraceId.CopyTo(traceId);
      Activity.Current.SpanId.CopyTo(spanId);
    }

    return new DomainEvent
    {
      Metadata = new DomainEventMetadata
      {
        EventId = Guid.CreateVersion7(),
        StreamType = typeof(TEventBase).GetTypeName(),
        StreamId = streamId,
        StreamVersion = streamVersion,
        TenantId = tenantId,
        CreatedAt = createdAt,
        CreatedByUserId = userId,
        TraceId = traceId,
        SpanId = spanId,
      },
      Payload = payload,
    };
  }
}
