namespace Flensrocker.EventSourcing.Abstractions.PersistentProjection;

public interface IPersistentProjection
{
  Task RebuildAsync();
}
