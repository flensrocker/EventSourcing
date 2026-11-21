using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Flensrocker.EventSourcing.Abstractions.PersistentProjection;

public static partial class PersistentProjectionExtensions
{
  public static IServiceCollection AddPersistentProjection<TProjection>(
    this IServiceCollection services,
    string projectionTypeName,
    int currentVersion
  )
    where TProjection : class, IPersistentProjection
  {
    var options = PersistentProjectionOptions.Create<TProjection>(projectionTypeName, currentVersion);
    services.AddSingleton(options);
    services.AddKeyedSingleton(projectionTypeName, options);

    services.AddKeyedScoped<TProjection>(options.TypeName);
    services.AddKeyedScoped<IPersistentProjection>(options.TypeName, (sp, key) => sp.GetRequiredKeyedService<TProjection>(key));

    return services;
  }

  public static async Task RebuildOutdatedPersistentProjectionsAsync(this IServiceProvider serviceProvider, bool forceRebuild)
  {
    var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(PersistentProjection));

    var timeProvider = serviceProvider.GetRequiredService<TimeProvider>();

    var projectionEventStore = serviceProvider.GetRequiredService<IDomainEventStore<PersistentProjectionEvent>>();
    var (_, projectionState) = await projectionEventStore
      .ReadStateFromStorage<PersistentProjectionState>(PersistentProjectionConstants.RegistryId, default)
      .ConfigureAwait(false);
    var stateVersionIndex = 0;

    var projectionOptionsList = serviceProvider.GetServices<PersistentProjectionOptions>().ToArray();

    foreach (var projectionOptions in projectionOptionsList)
    {
      var expectedVersion = projectionOptions.CurrentVersion;
      LogInfoVersion(logger, projectionOptions.TypeName, expectedVersion);

      if (projectionState.TryGetProjectionVersion(projectionOptions.TypeName, out var actualVersion))
      {
        if (expectedVersion < actualVersion)
        {
          LogErrorActualVersionIsGreaterThanExpected(logger, projectionOptions.TypeName, actualVersion, expectedVersion);
          if (!forceRebuild)
            continue;
        }
        else if (expectedVersion == actualVersion)
        {
          LogInfoActualVersionEqualsExpected(logger, projectionOptions.TypeName, actualVersion, expectedVersion);
          if (!forceRebuild)
            continue;
        }
      }
      else
      {
        actualVersion = 0;

        LogInfoProjectionRegistering(logger, projectionOptions.TypeName);

        var projectionRegistered = DomainEvent.Create<PersistentProjectionEvent>(
          PersistentProjectionConstants.PersistentProjectionEvent.Base,
          PersistentProjectionConstants.RegistryId,
          projectionState.Version + stateVersionIndex + 1,
          new PersistentProjectionEvent.ProjectionRegistered { TypeName = projectionOptions.TypeName },
          tenantId: null,
          userId: null,
          timeProvider.GetUtcNow()
        );

        if (
          await projectionEventStore
            .ApplyUpdatesToStorage(
              PersistentProjectionConstants.RegistryId,
              [projectionRegistered],
              projectionState.Version + stateVersionIndex
            )
            .ConfigureAwait(false)
        )
        {
          stateVersionIndex++;
        }
        else
        {
          LogErrorProjectionEvent(logger, projectionOptions.TypeName, nameof(PersistentProjectionEvent.ProjectionRegistered));
        }
      }

      if (actualVersion < expectedVersion)
        LogInfoActualVersionIsLessThanExpected(logger, projectionOptions.TypeName, actualVersion, expectedVersion);

      if (forceRebuild)
        LogInfoForcedRebuild(logger, projectionOptions.TypeName, actualVersion, expectedVersion);

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
            Forced = forceRebuild,
          },
          tenantId: null,
          userId: null,
          timeProvider.GetUtcNow()
        );

        if (
          await projectionEventStore
            .ApplyUpdatesToStorage(
              PersistentProjectionConstants.RegistryId,
              [projectionRebuilt],
              projectionState.Version + stateVersionIndex
            )
            .ConfigureAwait(false)
        )
        {
          stateVersionIndex++;
        }
        else
        {
          LogErrorProjectionEvent(logger, projectionOptions.TypeName, nameof(PersistentProjectionEvent.ProjectionRebuilt));
        }
      }
      catch (Exception ex)
      {
        LogErrorRebuildFailed(logger, ex, projectionOptions.TypeName);
      }
    }

    LogInfoRebuildFinished(logger);
  }

  [LoggerMessage(LogLevel.Information, Message = "Projection {ProjectionTypeName}: version {ExpectedVersion}")]
  internal static partial void LogInfoVersion(ILogger logger, string projectionTypeName, int expectedVersion);

  [LoggerMessage(
    LogLevel.Error,
    Message = "Projection {ProjectionTypeName}: actual version {ActualVersion} is higher than expected version {ExpectedVersion}"
  )]
  internal static partial void LogErrorActualVersionIsGreaterThanExpected(
    ILogger logger,
    string projectionTypeName,
    int actualVersion,
    int expectedVersion
  );

  [LoggerMessage(
    LogLevel.Information,
    Message = "Projection {ProjectionTypeName}: actual version {ActualVersion} equals expected version {ExpectedVersion}"
  )]
  internal static partial void LogInfoActualVersionEqualsExpected(
    ILogger logger,
    string projectionTypeName,
    int actualVersion,
    int expectedVersion
  );

  [LoggerMessage(LogLevel.Information, Message = "Projection {ProjectionTypeName}: no version yet, registering")]
  internal static partial void LogInfoProjectionRegistering(ILogger logger, string projectionTypeName);

  [LoggerMessage(LogLevel.Information, Message = "Projection {ProjectionTypeName}: error on storing event {EventName}")]
  internal static partial void LogErrorProjectionEvent(ILogger logger, string projectionTypeName, string eventName);

  [LoggerMessage(
    LogLevel.Information,
    Message = "Projection {ProjectionTypeName}: rebuilding, because actual version {ActualVersion} is less than expected version {ExpectedVersion}"
  )]
  internal static partial void LogInfoActualVersionIsLessThanExpected(
    ILogger logger,
    string projectionTypeName,
    int actualVersion,
    int expectedVersion
  );

  [LoggerMessage(
    LogLevel.Information,
    Message = "Projection {ProjectionTypeName}: forced rebuild, actual version {ActualVersion}, expected version {ExpectedVersion}"
  )]
  internal static partial void LogInfoForcedRebuild(ILogger logger, string projectionTypeName, int actualVersion, int expectedVersion);

  [LoggerMessage(LogLevel.Error, Message = "Projection {ProjectionTypeName}: rebuild failed")]
  internal static partial void LogErrorRebuildFailed(ILogger logger, Exception ex, string projectionTypeName);

  [LoggerMessage(LogLevel.Information, Message = "Finished persistent projections rebuild")]
  internal static partial void LogInfoRebuildFinished(ILogger logger);
}
