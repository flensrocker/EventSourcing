using Flensrocker.EventSourcing.Abstractions;
using Flensrocker.EventSourcing.Persistence.EntityFrameworkCore;
using Flensrocker.EventSourcing.Persistence.PostgreSQL;
using Flensrocker.ToDo.Abstractions;
using Flensrocker.ToDo.Persistence.PostgreSQL;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Playground.ServiceDefaults;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

var globalJsonOptions = new EventSourcingJsonSerializerOptions().AddToDoJsonOptions();

builder.Services.AddFlensrockerEventSourcingCore();
builder
  .Services.AddPostgreSQLDomainEventStorage(() =>
  {
    return new EntityFrameworkCoreDomainEventStorageOptions { ConnectionName = ServiceKeys.PostgresDb, JsonOptions = globalJsonOptions };
  })
  .ForToDoEvents();

builder.Services.AddToDoPostgreSQLPersistence(() =>
{
  return new ToDoPostgreSQLPersistenceOptions { ConnectionName = ServiceKeys.PostgresDb };
});

builder.Services.AddHostedService<MigrationService>();

builder.Build().Run();
