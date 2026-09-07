namespace Flensrocker.EventSourcing.Abstractions.PersistentProjection;

internal static class PersistentProjectionConstants
{
  internal static readonly Guid RegistryId = Guid.Empty;

  internal static class PersistentProjectionEvent
  {
    internal const string Base = "Flensrocker_EventSourcing_PersistentProjectionEvent";
    internal const string ProjectionRegistered = $"{Base}_ProjectionRegistered";
    internal const string ProjectionRebuilt = $"{Base}_ProjectionRebuilt";
  }
}
