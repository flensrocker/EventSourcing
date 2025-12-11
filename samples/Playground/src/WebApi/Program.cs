using Flensrocker.EventSourcing.Abstractions;
using Flensrocker.EventSourcing.Orleans.Abstractions;
using Flensrocker.EventSourcing.Persistence.PostgreSQL;
using Flensrocker.ToDo.Abstractions;
using Flensrocker.ToDo.Persistence.PostgreSQL;
using Flensrocker.ToDo.WebApi;
using NATS.Client.Core;
using Orleans.Streaming.NATS.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var globalJsonOptions = EventSourcingJsonSerializerOptions.Default
  .AddToDoJsonOptions();

builder.AddKeyedRedisClient("redis");
builder.AddKeyedAzureQueueServiceClient("azure-queues-streams");

builder.Services.AddFlensrockerEventSourcingCore();
builder.Services.AddPostgreSQLDomainEventStorage(() => new PostgreSQLDomainEventStorageOptions
{
  ConnectionName = "playground-db",
  JsonOptions = globalJsonOptions,
}).ForDomainEvents<ToDoEvent>();

builder.Services.AddToDoPostgreSQLPersistence(() => new ToDoPostgreSQLPersistenceOptions
{
  ConnectionName = "playground-db",
});

builder.Host.UseOrleansClient(clientBuilder =>
{
  //var npgsqlConnectionString = builder.Configuration.GetConnectionString("playground-db") ?? throw new InvalidOperationException("Npgsql connection string not configured.");

  //clientBuilder.AddAdoNetStreams(OrleansConstants.DomainEventStreamProviderName, options =>
  //{
  //  options.Invariant = "Npgsql";
  //  options.ConnectionString = npgsqlConnectionString;
  //});

  //var natsConnectionString = builder.Configuration.GetConnectionString("nats") ?? throw new InvalidOperationException("NATS connection string not configured.");
  //clientBuilder.AddNatsStreams(OrleansConstants.DomainEventStreamProviderName, options =>
  //{
  //  options.StreamName = OrleansConstants.DomainEventStreamProviderName;
  //  options.JsonSerializerOptions = globalJsonOptions;
  //  options.NatsClientOptions = NatsOpts.Default with
  //  {
  //    Url = natsConnectionString,
  //  };
  //});
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

app.MapToDo();

app.Run();
