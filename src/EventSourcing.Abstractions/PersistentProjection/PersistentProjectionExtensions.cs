using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Flensrocker.EventSourcing.Abstractions.PersistentProjection;

public static class PersistentProjectionExtensions
{
  public static IServiceCollection AddPersistentProjection<TProjection>(this IServiceCollection services, string projectionTypeName, int currentVersion)
    where TProjection : class, IPersistentProjection
  {
    var options = PersistentProjectionOptions.Create<TProjection>(projectionTypeName, currentVersion);
    services.AddSingleton(options);
    services.AddKeyedSingleton(projectionTypeName, options);

    services.AddKeyedScoped<TProjection>(options.TypeName);
    services.AddKeyedScoped<IPersistentProjection>(options.TypeName, (sp, key) => sp.GetRequiredKeyedService<TProjection>(key));

    return services;
  }

  public static async Task RebuildOutdatedPersistentProjectionsAsync(this IServiceProvider serviceProvider)
  {
    var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(PersistentProjection));

    var timeProvider = serviceProvider.GetRequiredService<TimeProvider>();

    var projectionEventStore = serviceProvider.GetRequiredService<IDomainEventStorage<PersistentProjectionEvent>>();
    var (_, projectionState) = await projectionEventStore.ReadStateFromStorage<PersistentProjectionState>(PersistentProjectionConstants.RegistryId, default).ConfigureAwait(false);
    var stateVersionIndex = 0;

    var projectionOptionsList = serviceProvider.GetServices<PersistentProjectionOptions>().ToArray();

    foreach (var projectionOptions in projectionOptionsList)
    {
      var expectedVersion = projectionOptions.CurrentVersion;
      logger.LogInformation("Projection {ProjectionTypeName}: version {ExpectedVersion}", projectionOptions.TypeName, expectedVersion);

      if (projectionState.TryGetProjectionVersion(projectionOptions.TypeName, out var actualVersion))
      {
        if (expectedVersion < actualVersion)
        {
          logger.LogError("Projection {ProjectionTypeName}: actual version {ActualVersion} is higher than expected version {ExpectedVersion}", projectionOptions.TypeName, actualVersion, expectedVersion);
          continue;
        }

        if (expectedVersion == actualVersion)
        {
          logger.LogInformation("Projection {ProjectionTypeName}: actual version {ActualVersion} equals expected version {ExpectedVersion}", projectionOptions.TypeName, actualVersion, expectedVersion);
          continue;
        }
      }
      else
      {
        actualVersion = 0;

        logger.LogInformation("Projection {ProjectionTypeName}: no version yet, registering", projectionOptions.TypeName);

        var projectionRegistered = DomainEvent.Create<PersistentProjectionEvent>(
          PersistentProjectionConstants.PersistentProjectionEvent.Base,
          PersistentProjectionConstants.RegistryId,
          projectionState.Version + stateVersionIndex + 1,
          new PersistentProjectionEvent.ProjectionRegistered
          {
            TypeName = projectionOptions.TypeName,
          },
          tenantId: null,
          userId: null,
          timeProvider.GetUtcNow());

        if (await projectionEventStore.ApplyUpdatesToStorage(PersistentProjectionConstants.RegistryId, [projectionRegistered], projectionState.Version + stateVersionIndex).ConfigureAwait(false))
          stateVersionIndex++;
        else
          logger.LogError("Projection {ProjectionTypeName}: error on storing event {EventName}", projectionOptions.TypeName, nameof(PersistentProjectionEvent.ProjectionRegistered));
      }

      logger.LogInformation("Projection {ProjectionTypeName}: rebuilding, because actual version {ActualVersion} is less than expected version {ExpectedVersion}", projectionOptions.TypeName, actualVersion, expectedVersion);

      try
      {
        await using var rebuildScope = serviceProvider.CreateAsyncScope();

        var projection = rebuildScope.ServiceProvider.GetRequiredKeyedService<IPersistentProjection>(projectionOptions.TypeName);

        await projection.RebuildAsync().ConfigureAwait(false);

        var projectionRebuilt = DomainEvent.Create<PersistentProjectionEvent>(
          PersistentProjectionConstants.PersistentProjectionEvent.Base,
          PersistentProjectionConstants.RegistryId,
          projectionState.Version + stateVersionIndex + 1,
          new PersistentProjectionEvent.ProjectionRebuilt
          {
            TypeName = projectionOptions.TypeName,
            Version = projectionOptions.CurrentVersion,
          },
          tenantId: null,
          userId: null,
          timeProvider.GetUtcNow());

        if (await projectionEventStore.ApplyUpdatesToStorage(PersistentProjectionConstants.RegistryId, [projectionRebuilt], projectionState.Version + stateVersionIndex).ConfigureAwait(false))
          stateVersionIndex++;
        else
          logger.LogError("Projection {ProjectionTypeName}: error on storing event {EventName}", projectionOptions.TypeName, nameof(PersistentProjectionEvent.ProjectionRebuilt));
      }
      catch (Exception ex)
      {
        logger.LogError(ex, "Projection {ProjectionTypeName}: rebuild failed", projectionOptions.TypeName);
      }
    }

    logger.LogInformation("Finished persistent projections rebuild");
  }
}
