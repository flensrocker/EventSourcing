using Flensrocker.Domain.Abstractions;
using Flensrocker.Domain.Orleans.Abstractions;
using Flensrocker.Domain.Persistence.PostgreSQL;
using Flensrocker.ToDo.Abstractions;
using Flensrocker.ToDo.Orleans.Server;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using NATS.Client.Core;
using Orleans.Serialization;
using Orleans.Streaming.NATS.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

var globalJsonOptions = DomainJsonSerializerOptions.Default
  .AddToDoJsonOptions();

builder.AddKeyedRedisClient("redis");

builder.Services.AddPostgreSQLDomainEventStorage(() => new PostgreSQLDomainEventStorageOptions<ToDoEvent>
{
  ConnectionName = "playground",
  JsonOptions = globalJsonOptions,
});

builder.UseOrleans(siloBuilder =>
{
  siloBuilder.Services.AddSerializer(serializerBuilder =>
  {
    serializerBuilder.AddJsonSerializer(globalJsonOptions.Supports, globalJsonOptions);
  });

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

  var natsConnectionString = builder.Configuration.GetConnectionString("nats") ?? throw new InvalidOperationException("NATS connection string not configured.");
  siloBuilder.AddNatsStreams(DomainConstants.StreamProviderDomainMessages, options =>
  {
    options.StreamName = DomainConstants.StreamProviderDomainMessages;
    options.JsonSerializerOptions = globalJsonOptions;
    options.NatsClientOptions = NatsOpts.Default with
    {
      Url = natsConnectionString,
    };
  });

  siloBuilder.AddToDo();
});

await builder.Build().RunAsync();
