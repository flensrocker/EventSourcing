using Flensrocker.EventSourcing.Abstractions;
using Flensrocker.EventSourcing.Orleans.Abstractions;
using Flensrocker.EventSourcing.Persistence.PostgreSQL;
using Flensrocker.ToDo.Abstractions;
using Flensrocker.ToDo.Orleans.Client;
using Flensrocker.ToDo.Orleans.Server;
using Flensrocker.ToDo.Persistence.PostgreSQL;
using Flensrocker.ToDo.WebApi;

var builder = WebApplication.CreateBuilder(args);

var globalJsonOptions = EventSourcingJsonSerializerOptions.Default.AddToDoJsonOptions();

builder.Services.AddFlensrockerEventSourcingCore();
builder
  .Services.AddPostgreSQLDomainEventStorage(() =>
    new PostgreSQLDomainEventStorageOptions { ConnectionName = "DefaultConnection", JsonOptions = globalJsonOptions }
  )
  .ForToDoEvents();

builder
  .Services.AddToDoOrleansClient()
  .AddToDoPostgreSQLPersistence(() => new ToDoPostgreSQLPersistenceOptions { ConnectionName = "DefaultConnection" });

builder.Services.AddPostgreSQLDomainMessageSubscriber(() =>
{
  return new PostgreSQLMessageSubscriberOptions { ConnectionName = "DefaultConnection", JsonOptions = globalJsonOptions };
});

builder.UseOrleans(siloBuilder =>
{
  siloBuilder.UseLocalhostClustering();
  siloBuilder.AddMemoryGrainStorage("PubSubStore");

  siloBuilder.AddCustomStorageBasedLogConsistencyProvider(OrleansConstants.LogStorageProviderName);

  siloBuilder.AddMemoryStreams(OrleansConstants.DomainEventStreamProviderName);

  siloBuilder.AddToDoOrleans();
});

builder.Services.AddOrleansDomainEventStreamPublisher();

builder.Services.ConfigureHttpJsonOptions(jsonOptions =>
{
  jsonOptions.SerializerOptions.AllowOutOfOrderMetadataProperties = globalJsonOptions.Options.AllowOutOfOrderMetadataProperties;
  jsonOptions.SerializerOptions.PropertyNamingPolicy = globalJsonOptions.Options.PropertyNamingPolicy;
  foreach (var typeResolver in globalJsonOptions.Options.TypeInfoResolverChain)
    jsonOptions.SerializerOptions.TypeInfoResolverChain.Add(typeResolver);
});

using var app = builder.Build();

await using (var migrationScope = app.Services.CreateAsyncScope())
{
  var logger = migrationScope.ServiceProvider.GetRequiredService<ILogger<Program>>();
  logger.LogInformation("Starting Migrations");

  await migrationScope.ServiceProvider.RunDbMigrationsAsync();

  logger.LogInformation("Finished Migrations");
}

app.MapToDo();

app.Run();
