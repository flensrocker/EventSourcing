namespace Flensrocker.EventSourcing.Abstractions.PersistentProjection;

public static class PersistentProjectionConstants
{
  public static readonly Guid RegistryId = Guid.Empty;

  public static class PersistentProjectionEvent
  {
    public const string Base = $"{EventSourcingConstants.AliasBase}_PersistentProjectionEvent";
    public const string ProjectionRegistered = $"{Base}_ProjectionRegistered";
    public const string ProjectionRebuilt = $"{Base}_ProjectionRebuilt";
  }
}
