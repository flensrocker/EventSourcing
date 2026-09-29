using Flensrocker.EventSourcing.Abstractions;
using Flensrocker.EventSourcing.Abstractions.PersistentProjection;

internal class MigrationService(IServiceScopeFactory scopeFactory, ILogger<MigrationService> logger) : IHostedService
{
  private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
  private readonly ILogger<MigrationService> _logger = logger;

  public async Task StartAsync(CancellationToken cancellationToken)
  {
    _logger.LogInformation("Starting Migrations");

    await using (var migrationScope = _scopeFactory.CreateAsyncScope())
      await migrationScope.ServiceProvider.RunDbMigrationsAsync();

    _logger.LogInformation("Finished Migrations");

    _logger.LogInformation("Starting Rebuilds");

    await using (var rebuildScope = _scopeFactory.CreateAsyncScope())
      await rebuildScope.ServiceProvider.RebuildOutdatedPersistentProjectionsAsync();

    _logger.LogInformation("Finished Rebuilds");
  }

  public Task StopAsync(CancellationToken cancellationToken)
  {
    return Task.CompletedTask;
  }
}
