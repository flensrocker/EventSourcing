using System.Text.Json.Serialization;

namespace Flensrocker.EventSourcing.Abstractions.PersistentProjection;

[GenerateSerializer]
[Alias(PersistentProjectionConstants.PersistentProjectionEventTypeName)]
[JsonDerivedType(typeof(ProjectionRegistered), nameof(ProjectionRegistered))]
[JsonDerivedType(typeof(ProjectionRebuilt), nameof(ProjectionRebuilt))]
public abstract record PersistentProjectionEvent
{
  private PersistentProjectionEvent() { }

  [GenerateSerializer]
  [Alias($"{PersistentProjectionConstants.PersistentProjectionEventTypeName}.ProjectionRegistered")]
  public sealed record ProjectionRegistered : PersistentProjectionEvent
  {
    [Id(0)]
    public required string TypeName { get; init; }
  }

  [GenerateSerializer]
  [Alias($"{PersistentProjectionConstants.PersistentProjectionEventTypeName}.ProjectionRebuilt")]
  public sealed record ProjectionRebuilt : PersistentProjectionEvent
  {
    [Id(0)]
    public required string TypeName { get; init; }

    [Id(1)]
    public required int Version { get; init; }
  }
}
