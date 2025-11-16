using Flensrocker.EventSourcing.Abstractions;
using Flensrocker.EventSourcing.Orleans.PostgreSQL;
using Flensrocker.EventSourcing.Persistence.PostgreSQL;
using Flensrocker.ToDo.Abstractions;
using Flensrocker.ToDo.Persistence.PostgreSQL;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

var globalJsonOptions = EventSourcingJsonSerializerOptions.Default
  .AddToDoJsonOptions();

builder.Services.AddOrleansPostgreSQL(() =>
{
  return new OrleansPostgreSQLOptions
  {
    ConnectionName = "playground",
  };
});

builder.Services.AddPostgreSQLDomainEventStorage(() =>
{
  return new PostgreSQLDomainEventStorageOptions
  {
    ConnectionName = "playground",
    JsonOptions = globalJsonOptions,
  };
}).ForDomainEvents<ToDoEvent>();

builder.Services.AddToDoPostgreSQLPersistence(() =>
{
  return new ToDoPostgreSQLPersistenceOptions
  {
    ConnectionName = "playground",
  };
});

builder.Services.AddHostedService<MigrationService>();

builder.Build().Run();

internal class MigrationService(
  IServiceScopeFactory scopeFactory,
  IHostApplicationLifetime hostLifetime,
  ILogger<MigrationService> logger)
  : BackgroundService
{
  private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
  private readonly IHostApplicationLifetime _hostLifetime = hostLifetime;
  private readonly ILogger<MigrationService> _logger = logger;

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    _logger.LogInformation("Starting Migrations");

    await using (var scope = _scopeFactory.CreateAsyncScope())
    {
      _logger.LogInformation("Migrate Orleans.PostgreSQL");
      await scope.ServiceProvider.MigrateOrleansPostgreSQLAsync();
    }

    await using (var scope = _scopeFactory.CreateAsyncScope())
    {
      _logger.LogInformation("Migrate Domain-Event-Storage");
      await scope.ServiceProvider.MigratePostgreSQLDomainEventStorageAsync();
    }

    await using (var scope = _scopeFactory.CreateAsyncScope())
    {
      _logger.LogInformation("Migrate ToDo-Persistence");
      await scope.ServiceProvider.MigrateToDoPostgreSQLPersistenceAsync();
    }

    _logger.LogInformation("Finished Migrations");

    _hostLifetime.StopApplication();
  }
}
