using Flensrocker.EventSourcing.Abstractions;
using Flensrocker.EventSourcing.Orleans.Abstractions;
using Flensrocker.EventSourcing.Persistence.EntityFrameworkCore;
using Flensrocker.EventSourcing.Persistence.PostgreSQL;
using Flensrocker.Measure.Orleans.Abstractions;
using Flensrocker.Measure.Orleans.Server;
using Flensrocker.ToDo.Abstractions;
using Flensrocker.ToDo.Domain;
using Flensrocker.ToDo.Orleans.Server;
using Flensrocker.ToDo.Persistence.PostgreSQL;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using NATS.Client.Core;
using Orleans.Streaming.NATS.Hosting;
using Playground.ServiceDefaults;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

var globalJsonOptions = new EventSourcingJsonSerializerOptions().AddMeasureJsonOptions().AddToDoJsonOptions().MakeReadOnly();

builder.AddKeyedRedisClient(ServiceKeys.OrleansClustering);

builder.Services.AddFlensrockerEventSourcingCore(globalJsonOptions);
builder
  .Services.AddPostgreSQLDomainEventStorage(() =>
    new EntityFrameworkCoreDomainEventStorageOptions { ConnectionName = ServiceKeys.PostgresDb }
  )
  .ForToDoEvents();

builder
  .Services.AddToDo()
  .AddToDoPostgreSQLPersistence(() => new ToDoPostgreSQLPersistenceOptions { ConnectionName = ServiceKeys.PostgresDb });

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

await builder.Build().RunAsync();
