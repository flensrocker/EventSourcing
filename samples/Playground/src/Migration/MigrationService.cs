using Flensrocker.EventSourcing.Abstractions;
using Flensrocker.EventSourcing.Abstractions.PersistentProjection;
using Flensrocker.EventSourcing.Orleans.PostgreSQL;
using Flensrocker.EventSourcing.Persistence.PostgreSQL;
using Flensrocker.ToDo.Abstractions;
using Flensrocker.ToDo.Persistence.PostgreSQL;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

internal class MigrationService(
  IServiceScopeFactory scopeFactory,
  IHostApplicationLifetime hostLifetime,
  ILogger<MigrationService> logger
) : BackgroundService
{
  private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
  private readonly IHostApplicationLifetime _hostLifetime = hostLifetime;
  private readonly ILogger<MigrationService> _logger = logger;

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    _logger.LogInformation("Starting Migrations");

    await using (var migrationScope = _scopeFactory.CreateAsyncScope())
      await migrationScope.ServiceProvider.RunDbMigrationsAsync();

    _logger.LogInformation("Finished Migrations");

    _logger.LogInformation("Starting Rebuilds");

    await using (var rebuildScope = _scopeFactory.CreateAsyncScope())
      await rebuildScope.ServiceProvider.RebuildOutdatedPersistentProjectionsAsync();

    _logger.LogInformation("Finished Rebuilds");

    _hostLifetime.StopApplication();
  }
}
