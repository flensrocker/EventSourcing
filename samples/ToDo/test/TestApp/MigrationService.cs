using Flensrocker.EventSourcing.Abstractions;
using Flensrocker.EventSourcing.Abstractions.PersistentProjection;

internal partial class MigrationService(IServiceScopeFactory scopeFactory, ILogger<MigrationService> logger) : IHostedService
{
  private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
  private readonly ILogger<MigrationService> _logger = logger;

  public async Task StartAsync(CancellationToken cancellationToken)
  {
    LogMigrationStarting(_logger);

    await using (var migrationScope = _scopeFactory.CreateAsyncScope())
      await migrationScope.ServiceProvider.RunDbMigrationsAsync();

    LogMigrationFinished(_logger);

    LogRebuildStarting(_logger);

    await using (var rebuildScope = _scopeFactory.CreateAsyncScope())
      await rebuildScope.ServiceProvider.RebuildOutdatedPersistentProjectionsAsync();

    LogRebuildFinished(_logger);
  }

  public Task StopAsync(CancellationToken cancellationToken)
  {
    return Task.CompletedTask;
  }

  [LoggerMessage(Level = LogLevel.Information, Message = "Starting Migrations")]
  internal static partial void LogMigrationStarting(ILogger logger);

  [LoggerMessage(Level = LogLevel.Information, Message = "Finished Migrations")]
  internal static partial void LogMigrationFinished(ILogger logger);

  [LoggerMessage(Level = LogLevel.Information, Message = "Starting Rebuilds")]
  internal static partial void LogRebuildStarting(ILogger logger);

  [LoggerMessage(Level = LogLevel.Information, Message = "Finished Rebuilds")]
  internal static partial void LogRebuildFinished(ILogger logger);
}
