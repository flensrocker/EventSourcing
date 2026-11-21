using Flensrocker.EventSourcing.Abstractions;
using Flensrocker.EventSourcing.Persistence.EntityFrameworkCore;
using Flensrocker.EventSourcing.Persistence.PostgreSQL;
using Flensrocker.ToDo.Abstractions;
using Flensrocker.ToDo.Persistence.PostgreSQL;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Playground.ServiceDefaults;

var builder = Host.CreateApplicationBuilder(args);

var forceRebuild = args.Any(arg => "--force".Equals(arg, StringComparison.OrdinalIgnoreCase));

builder.AddServiceDefaults();

var globalJsonOptions = new EventSourcingJsonSerializerOptions().AddToDoJsonOptions();

builder.Services.AddFlensrockerEventSourcingCore(globalJsonOptions);
builder
  .Services.AddPostgreSQLDomainEventStore(() =>
  {
    return new EntityFrameworkCoreDomainEventStoreOptions { ConnectionName = ServiceKeys.PostgresDb };
  })
  .ForToDoEvents();

builder.Services.AddToDoPostgreSQLPersistence(() =>
{
  return new ToDoPostgreSQLPersistenceOptions { ConnectionName = ServiceKeys.PostgresDb };
});

builder.Services.AddSingleton(new MigrationOptions { ForceRebuild = forceRebuild }).AddHostedService<MigrationService>();

builder.Build().Run();
