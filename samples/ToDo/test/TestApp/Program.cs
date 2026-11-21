using System.Security.Cryptography;
using Flensrocker.EventSourcing.Abstractions;
using Flensrocker.EventSourcing.Extensions.AspNetCore;
using Flensrocker.EventSourcing.Orleans.Abstractions;
using Flensrocker.EventSourcing.Persistence.EntityFrameworkCore;
using Flensrocker.EventSourcing.Persistence.PostgreSQL;
using Flensrocker.ToDo.Abstractions;
using Flensrocker.ToDo.Domain;
using Flensrocker.ToDo.Orleans.Client;
using Flensrocker.ToDo.Orleans.Server;
using Flensrocker.ToDo.Persistence.PostgreSQL;
using Flensrocker.ToDo.WebApi;
using OpenTelemetry.Logs;

var builder = WebApplication.CreateBuilder(args);

// Build JsonSerializerOptions for all relevant parts of the application
var globalJsonOptions = new EventSourcingJsonSerializerOptions().AddAspNetCoreJsonOptions().AddToDoJsonOptions().MakeReadOnly();

var connectionName = "DefaultConnection";

builder.Logging.AddOpenTelemetry(logging =>
{
  logging.IncludeFormattedMessage = true;
  logging.IncludeScopes = true;
  logging.AddConsoleExporter();
});
builder
  .Services.AddOpenTelemetry()
  .WithMetrics(metrics => metrics.AddMeter(EventSourcingConstants.MeterName))
  .WithTracing(tracing => tracing.AddSource(EventSourcingConstants.TraceSourceName));

// Register the migration service as early as possible so it runs before all other hosted services
builder.Services.AddHostedService<MigrationService>();

// Activate core event sourcing services and PostgreSQL storage for "ToDo" events
builder
  .Services.AddFlensrockerEventSourcingCore(globalJsonOptions)
  .AddPostgreSQLDomainEventStorage(() => new EntityFrameworkCoreDomainEventStorageOptions { ConnectionName = connectionName })
  .ForToDoEvents();

// Activate in-process outbox on PostgreSQL with Orleans Streaming publisher
builder
  .Services.AddPostgreSQLDomainMessageSubscriber(() =>
  {
    return new PostgreSQLMessageSubscriberOptions { ConnectionName = connectionName };
  })
  .AddOrleansDomainEventStreamPublisher();

// Add all "ToDo" services for in-process hosting
builder
  .Services.AddToDo()
  .AddToDoOrleansClient()
  .AddToDoPostgreSQLPersistence(() => new ToDoPostgreSQLPersistenceOptions { ConnectionName = connectionName })
  .AddToDoWebApi();

// Activate in-process Orleans silo
builder.UseOrleans(siloBuilder =>
{
  // Real apps should use a dedicated storage provider for clustering like Redis/Garnet, Azure Tables etc.
  // The TestApp generates random ports so multiple instances can run in parallel
  var siloPort = RandomNumberGenerator.GetInt32(11000, 12000);
  var gatewayPort = RandomNumberGenerator.GetInt32(30000, 31000);
  siloBuilder.UseLocalhostClustering(siloPort, gatewayPort);

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

// Map the "ToDo" endpoints for the integration tests
app.MapToDo();

app.Run();
