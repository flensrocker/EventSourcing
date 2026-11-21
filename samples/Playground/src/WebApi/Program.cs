using Flensrocker.EventSourcing.Abstractions;
using Flensrocker.EventSourcing.Extensions.AspNetCore;
using Flensrocker.EventSourcing.Orleans.Abstractions;
using Flensrocker.EventSourcing.Persistence.EntityFrameworkCore;
using Flensrocker.EventSourcing.Persistence.PostgreSQL;
using Flensrocker.Measure.Orleans.Abstractions;
using Flensrocker.Measure.WebApi;
using Flensrocker.ToDo.Abstractions;
using Flensrocker.ToDo.Orleans.Client;
using Flensrocker.ToDo.Persistence.PostgreSQL;
using Flensrocker.ToDo.WebApi;
using NATS.Client.Core;
using Orleans.Streaming.NATS.Hosting;
using Playground.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var globalJsonOptions = new EventSourcingJsonSerializerOptions().AddAspNetCoreJsonOptions().AddMeasureJsonOptions().AddToDoJsonOptions();

builder.AddKeyedRedisClient(ServiceKeys.OrleansClustering);

builder.Services.AddFlensrockerEventSourcingCore(globalJsonOptions);
builder
  .Services.AddPostgreSQLDomainEventStore(() => new EntityFrameworkCoreDomainEventStoreOptions { ConnectionName = ServiceKeys.PostgresDb })
  .ForToDoEvents();

builder
  .Services.AddToDoOrleansClient()
  .AddToDoPostgreSQLPersistence(() => new ToDoPostgreSQLPersistenceOptions { ConnectionName = ServiceKeys.PostgresDb })
  .AddToDoWebApi();

builder.Host.UseOrleansClient(clientBuilder =>
{
  var npgsqlConnectionString = builder.Configuration.GetConnectionString(ServiceKeys.PostgresDb)
    ?? throw new InvalidOperationException("Npgsql connection string not configured.");

  var natsConnectionString = builder.Configuration.GetConnectionString(ServiceKeys.OrleansStreaming)
    ?? throw new InvalidOperationException("NATS connection string not configured.");
  clientBuilder.AddNatsStreams(OrleansConstants.DomainEventStreamProviderName, options =>
  {
    options.StreamName = OrleansConstants.DomainEventStreamProviderName;
    options.JsonSerializerOptions = globalJsonOptions.Options;
    options.NatsClientOptions = NatsOpts.Default with { Url = natsConnectionString };
  });
});

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
