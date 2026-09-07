namespace Flensrocker.EventSourcing.Abstractions.PersistentProjection;

internal class PersistentProjectionState : IDomainEventProjectable, IDomainEventVersionable
{
  private readonly Dictionary<string, int> _typeNameVersionMap = new();

  public int Version { get; private set; }

  public void Apply(DomainEvent @event)
  {
    Version = @event.Metadata.StreamVersion;

    if (@event.Payload is PersistentProjectionEvent.ProjectionRegistered registered)
      _typeNameVersionMap[registered.TypeName] = 1;
    else if (@event.Payload is PersistentProjectionEvent.ProjectionRebuilt rebuilt)
      _typeNameVersionMap[rebuilt.TypeName] = rebuilt.Version;
  }

  internal bool TryGetProjectionVersion(string typeName, out int projectionVersion) =>
    _typeNameVersionMap.TryGetValue(typeName, out projectionVersion);
}
