using Flensrocker.EventSourcing.Abstractions;
using Flensrocker.EventSourcing.Abstractions.PersistentProjection;
using Flensrocker.EventSourcing.Orleans.Abstractions;
using Flensrocker.EventSourcing.Persistence.EntityFrameworkCore;
using Flensrocker.EventSourcing.Persistence.PostgreSQL;
using Flensrocker.ToDo.Abstractions;
using Flensrocker.ToDo.Domain;
using Flensrocker.ToDo.Orleans.Client;
using Flensrocker.ToDo.Orleans.Server;
using Flensrocker.ToDo.Persistence.PostgreSQL;
using Flensrocker.ToDo.WebApi;

var builder = WebApplication.CreateBuilder(args);

// Build JsonSerializerOptions for all relevant parts of the application
var globalJsonOptions = new EventSourcingJsonSerializerOptions().AddToDoJsonOptions();

// Activate core event sourcing services and PostgreSQL storage for "ToDo" events
builder
  .Services.AddFlensrockerEventSourcingCore()
  .AddPostgreSQLDomainEventStorage(() =>
    new EntityFrameworkCoreDomainEventStorageOptions { ConnectionName = "DefaultConnection", JsonOptions = globalJsonOptions }
  )
  .ForToDoEvents();

// Activate in-process outbox on PostgreSQL with Orleans Streaming publisher
builder
  .Services.AddPostgreSQLDomainMessageSubscriber(() =>
  {
    return new PostgreSQLMessageSubscriberOptions { ConnectionName = "DefaultConnection", JsonOptions = globalJsonOptions };
  })
  .AddOrleansDomainEventStreamPublisher();

// Add all "ToDo" services for in-process hosting
builder
  .Services.AddToDo()
  .AddToDoOrleansClient()
  .AddToDoPostgreSQLPersistence(() => new ToDoPostgreSQLPersistenceOptions { ConnectionName = "DefaultConnection" })
  .AddToDoWebApi();

// Activate in-process Orleans silo
builder.UseOrleans(siloBuilder =>
{
  // Real apps should use a dedicated storage provider for clustering like Redis/Garnet, Azure Tables etc.
  siloBuilder.UseLocalhostClustering();

  // Implicit subscription don't need persistent storage
  // The explicit subscriptions used in this example don't need to survive grain activiation
  // If using durable explicit subscriptions a persistent storage provider for "PubSubStore" must be used
  siloBuilder.AddMemoryGrainStorage("PubSubStore");

  // Add the event sourcing storage provider so it can be used on the CommandHandlerGrain
  siloBuilder.AddCustomStorageBasedLogConsistencyProvider(OrleansConstants.LogStorageProviderName);

  // Real apps should use a dedicated streaming provider like NATS, Azure Queues etc.
  siloBuilder.AddMemoryStreams(OrleansConstants.DomainEventStreamProviderName);

  // Setup the "ToDo" grains
  siloBuilder.AddToDoOrleans();
});

// Configure JSON options for ASP.NET Core and HttpClient
builder.Services.ConfigureHttpJsonOptions(jsonOptions =>
{
  jsonOptions.SerializerOptions.AllowOutOfOrderMetadataProperties = globalJsonOptions.Options.AllowOutOfOrderMetadataProperties;
  jsonOptions.SerializerOptions.PropertyNamingPolicy = globalJsonOptions.Options.PropertyNamingPolicy;
  foreach (var typeResolver in globalJsonOptions.Options.TypeInfoResolverChain)
    jsonOptions.SerializerOptions.TypeInfoResolverChain.Add(typeResolver);
});

using var app = builder.Build();

// Create and migrate database on startup for easier setup
await using (var migrationScope = app.Services.CreateAsyncScope())
{
  var logger = app.Services.GetRequiredService<ILogger<Program>>();

  logger.LogInformation("Starting Migrations");
  await migrationScope.ServiceProvider.RunDbMigrationsAsync();
  logger.LogInformation("Finished Migrations");

  logger.LogInformation("Starting Rebuilds");
  await migrationScope.ServiceProvider.RebuildOutdatedPersistentProjectionsAsync();
  logger.LogInformation("Finished Rebuilds");
}

// Map the "ToDo" endpoints for the integration tests
app.MapToDo();

app.Run();
