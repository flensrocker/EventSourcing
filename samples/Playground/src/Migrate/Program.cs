using Flensrocker.Domain.Abstractions;
using Flensrocker.Domain.Persistence.PostgreSQL;
using Flensrocker.ToDo.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

var globalJsonOptions = DomainJsonSerializerOptions.Default
  .AddToDoJsonOptions();

builder.Services.AddPostgreSQLDomainEventStorage(() =>
{
  return new PostgreSQLDomainEventStorageOptions<ToDoEvent>
  {
    ConnectionName = "todo",
    JsonOptions = globalJsonOptions,
  };
});

var host = builder.Build();

await using (var scope = host.Services.CreateAsyncScope())
{
  var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

  logger.LogInformation("Migration starting");

  logger.LogInformation("Migrate DomainDbContext");
  await scope.ServiceProvider.MigratePostgreSQLDomainEventStorageAsync();

  logger.LogInformation("Migration finished");
}
