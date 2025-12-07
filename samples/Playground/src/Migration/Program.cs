using Flensrocker.EventSourcing.Abstractions;
using Flensrocker.EventSourcing.Orleans.PostgreSQL;
using Flensrocker.EventSourcing.Persistence.PostgreSQL;
using Flensrocker.ToDo.Abstractions;
using Flensrocker.ToDo.Persistence.PostgreSQL;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

var globalJsonOptions = EventSourcingJsonSerializerOptions.Default
  .AddToDoJsonOptions();

builder.Services.AddOrleansPostgreSQL(() =>
{
  return new OrleansPostgreSQLOptions
  {
    ConnectionName = "playground-db",
  };
});

builder.Services.AddFlensrockerEventSourcingCore();
builder.Services.AddPostgreSQLDomainEventStorage(() =>
{
  return new PostgreSQLDomainEventStorageOptions
  {
    ConnectionName = "playground-db",
    JsonOptions = globalJsonOptions,
  };
}).ForDomainEvents<ToDoEvent>();

builder.Services.AddToDoPostgreSQLPersistence(() =>
{
  return new ToDoPostgreSQLPersistenceOptions
  {
    ConnectionName = "playground-db",
  };
});

builder.Services.AddHostedService<MigrationService>();

builder.Build().Run();
