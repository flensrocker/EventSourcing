using Flensrocker.Domain.Abstractions;
using Flensrocker.Domain.Orleans.PostgreSQL;
using Flensrocker.Domain.Persistence.PostgreSQL;
using Flensrocker.ToDo.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

var globalJsonOptions = DomainJsonSerializerOptions.Default
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
  return new PostgreSQLDomainEventStorageOptions<ToDoEvent>
  {
    ConnectionName = "playground",
    JsonOptions = globalJsonOptions,
  };
});

var host = builder.Build();

await using (var scope = host.Services.CreateAsyncScope())
{
  var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

  logger.LogInformation("Starting Migrations");

  logger.LogInformation("Migrate Orleans.PostgreSQL");
  await scope.ServiceProvider.MigrateOrleansPostgreSQLAsync();

  logger.LogInformation("Migrate Domain-Event-Storage");
  await scope.ServiceProvider.MigratePostgreSQLDomainEventStorageAsync();

  logger.LogInformation("Finished Migrations");
}
