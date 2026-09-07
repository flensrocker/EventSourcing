using System.Text.Json.Serialization;

namespace Flensrocker.EventSourcing.Abstractions.PersistentProjection;

[GenerateSerializer]
[Alias(PersistentProjectionConstants.PersistentProjectionEvent.Base)]
[JsonDerivedType(typeof(ProjectionRegistered), PersistentProjectionConstants.PersistentProjectionEvent.ProjectionRegistered)]
[JsonDerivedType(typeof(ProjectionRebuilt), PersistentProjectionConstants.PersistentProjectionEvent.ProjectionRebuilt)]
public abstract record PersistentProjectionEvent
{
  private PersistentProjectionEvent() { }

  [GenerateSerializer]
  [Alias(PersistentProjectionConstants.PersistentProjectionEvent.ProjectionRegistered)]
  public sealed record ProjectionRegistered : PersistentProjectionEvent
  {
    [Id(0)]
    public required string TypeName { get; init; }
  }

  [GenerateSerializer]
  [Alias(PersistentProjectionConstants.PersistentProjectionEvent.ProjectionRebuilt)]
  public sealed record ProjectionRebuilt : PersistentProjectionEvent
  {
    [Id(0)]
    public required string TypeName { get; init; }

    [Id(1)]
    public required int Version { get; init; }
  }
}
