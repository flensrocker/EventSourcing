using Flensrocker.EventSourcing.Abstractions;
using Flensrocker.EventSourcing.Orleans.Abstractions;
using Flensrocker.EventSourcing.Persistence.PostgreSQL;
using Flensrocker.ToDo.Abstractions;
using Flensrocker.ToDo.Persistence.PostgreSQL;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using NATS.Client.Core;
using Orleans.Streaming.NATS.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

var globalJsonOptions = EventSourcingJsonSerializerOptions.Default
  .AddToDoJsonOptions();

builder.AddKeyedRedisClient("redis");

builder.Services.AddPostgreSQLDomainEventStorage(() => new PostgreSQLDomainEventStorageOptions
{
  ConnectionName = "playground",
  JsonOptions = globalJsonOptions,
}).ForDomainEvents<ToDoEvent>();

builder.Services.AddToDoPostgreSQLPersistence(() => new ToDoPostgreSQLPersistenceOptions
{
  ConnectionName = "playground",
});

builder.UseOrleans(siloBuilder =>
{
  var npgsqlConnectionString = builder.Configuration.GetConnectionString("playground") ?? throw new InvalidOperationException("Npgsql connection string not configured.");
  siloBuilder.AddAdoNetGrainStorage("Default", options =>
  {
    options.Invariant = "Npgsql";
    options.ConnectionString = npgsqlConnectionString;
  });
  siloBuilder.AddAdoNetGrainStorage("PubSubStore", options =>
  {
    options.Invariant = "Npgsql";
    options.ConnectionString = npgsqlConnectionString;
  });
  siloBuilder.AddCustomStorageBasedLogConsistencyProvider(OrleansConstants.LogStorageProviderName);

  var natsConnectionString = builder.Configuration.GetConnectionString("nats") ?? throw new InvalidOperationException("NATS connection string not configured.");
  siloBuilder.AddNatsStreams(OrleansConstants.DomainEventStreamProviderName, options =>
  {
    options.StreamName = OrleansConstants.DomainEventStreamProviderName;
    options.JsonSerializerOptions = globalJsonOptions;
    options.NatsClientOptions = NatsOpts.Default with
    {
      Url = natsConnectionString,
    };
  });
});

await builder.Build().RunAsync();
