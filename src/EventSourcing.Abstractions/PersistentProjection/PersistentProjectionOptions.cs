namespace Flensrocker.EventSourcing.Abstractions.PersistentProjection;

public class PersistentProjectionOptions
{
  public required string TypeName { get; init; }
  public required int CurrentVersion { get; init; }

  public static PersistentProjectionOptions Create<TProjection>(string projectionTypeName, int currentVersion)
  {
    if (currentVersion < 1)
      throw new ArgumentException("CurrentVersion must be greater than zero", nameof(currentVersion));

    return new PersistentProjectionOptions { TypeName = projectionTypeName, CurrentVersion = currentVersion };
  }
}
