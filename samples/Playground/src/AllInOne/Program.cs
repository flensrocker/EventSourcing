using Flensrocker.EventSourcing.Abstractions;
using Flensrocker.EventSourcing.Extensions.AspNetCore;
using Flensrocker.EventSourcing.Orleans.Abstractions;
using Flensrocker.EventSourcing.Persistence.EntityFrameworkCore;
using Flensrocker.EventSourcing.Persistence.PostgreSQL;
using Flensrocker.Measure.Orleans.Abstractions;
using Flensrocker.Measure.Orleans.Server;
using Flensrocker.Measure.WebApi;
using Flensrocker.ToDo.Abstractions;
using Flensrocker.ToDo.Orleans.Client;
using Flensrocker.ToDo.Orleans.Server;
using Flensrocker.ToDo.Persistence.PostgreSQL;
using Flensrocker.ToDo.WebApi;
using NATS.Client.Core;
using Orleans.Streaming.NATS.Hosting;
using Playground.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var globalJsonOptions = new EventSourcingJsonSerializerOptions()
  .AddAspNetCoreJsonOptions()
  .AddMeasureJsonOptions()
  .AddToDoJsonOptions()
  .MakeReadOnly();

builder.AddKeyedRedisClient(ServiceKeys.OrleansClustering);

builder.Services.AddFlensrockerEventSourcingCore(globalJsonOptions);
builder
  .Services.AddPostgreSQLDomainEventStorage(() =>
    new EntityFrameworkCoreDomainEventStorageOptions { ConnectionName = ServiceKeys.PostgresDb }
  )
  .ForToDoEvents();

builder
  .Services.AddToDoOrleansClient()
  .AddToDoPostgreSQLPersistence(() => new ToDoPostgreSQLPersistenceOptions { ConnectionName = ServiceKeys.PostgresDb });

builder.Services.AddPostgreSQLDomainMessageSubscriber(() =>
{
  return new PostgreSQLMessageSubscriberOptions { ConnectionName = ServiceKeys.PostgresDb };
});

builder.UseOrleans(siloBuilder =>
{
  siloBuilder.AddCustomStorageBasedLogConsistencyProvider(OrleansConstants.LogStorageProviderName);

  var natsConnectionString = builder.Configuration.GetConnectionString(ServiceKeys.OrleansStreaming)
    ?? throw new InvalidOperationException("NATS connection string not configured.");
  siloBuilder.AddNatsStreams(OrleansConstants.DomainEventStreamProviderName, options =>
  {
    options.StreamName = OrleansConstants.DomainEventStreamProviderName;
    options.JsonSerializerOptions = globalJsonOptions.Options;
    options.NatsClientOptions = NatsOpts.Default with { Url = natsConnectionString };
  });

  siloBuilder.AddMeasureOrleans();
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

app.MapDefaultEndpoints();

app.MapMeasure();
app.MapToDo();

app.Run();
