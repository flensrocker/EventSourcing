using Flensrocker.EventSourcing.Abstractions;
using Flensrocker.EventSourcing.Orleans.Abstractions;
using Flensrocker.EventSourcing.Persistence.PostgreSQL;
using Flensrocker.Measure.Orleans.Abstractions;
using Flensrocker.Measure.Orleans.Server;
using Flensrocker.ToDo.Abstractions;
using Flensrocker.ToDo.Domain;
using Flensrocker.ToDo.Orleans.Server;
using Flensrocker.ToDo.Persistence.PostgreSQL;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NATS.Client.Core;
using Orleans.Streaming.NATS.Hosting;
using Playground.ServiceDefaults;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

var globalJsonOptions = EventSourcingJsonSerializerOptions.Default.AddMeasureJsonOptions().AddToDoJsonOptions();

builder.AddKeyedRedisClient(ServiceKeys.OrleansClustering);
builder.AddKeyedAzureQueueServiceClient(ServiceKeys.OrleansStreaming);

builder.Services.AddFlensrockerEventSourcingCore();
builder
  .Services.AddPostgreSQLDomainEventStorage(() =>
    new PostgreSQLDomainEventStorageOptions { ConnectionName = ServiceKeys.PostgresDb, JsonOptions = globalJsonOptions }
  )
  .ForDomainEvents<ToDoEvent>();

builder
  .Services.AddToDo()
  .AddToDoPostgreSQLPersistence(() => new ToDoPostgreSQLPersistenceOptions { ConnectionName = ServiceKeys.PostgresDb });

builder.UseOrleans(siloBuilder =>
{
  var npgsqlConnectionString =
    builder.Configuration.GetConnectionString(ServiceKeys.PostgresDb)
    ?? throw new InvalidOperationException("Npgsql connection string not configured.");

  siloBuilder.AddAdoNetGrainStorage(
    "Default",
    options =>
    {
      options.Invariant = "Npgsql";
      options.ConnectionString = npgsqlConnectionString;
    }
  );

  siloBuilder.AddCustomStorageBasedLogConsistencyProvider(OrleansConstants.LogStorageProviderName);

  //siloBuilder.AddAdoNetStreams(OrleansConstants.DomainEventStreamProviderName, options =>
  //{
  //  options.Invariant = "Npgsql";
  //  options.ConnectionString = npgsqlConnectionString;
  //});

  //var natsConnectionString = builder.Configuration.GetConnectionString("nats") ?? throw new InvalidOperationException("NATS connection string not configured.");
  //siloBuilder.AddNatsStreams(OrleansConstants.DomainEventStreamProviderName, options =>
  //{
  //  options.StreamName = OrleansConstants.DomainEventStreamProviderName;
  //  options.JsonSerializerOptions = globalJsonOptions;
  //  options.NatsClientOptions = NatsOpts.Default with
  //  {
  //    Url = natsConnectionString,
  //  };
  //});

  siloBuilder.AddMeasureOrleans();
  siloBuilder.AddToDoOrleans();
});

await builder.Build().RunAsync();
