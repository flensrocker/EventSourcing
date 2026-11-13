using Flensrocker.EventSourcing.Abstractions;
using Flensrocker.EventSourcing.Orleans.Abstractions;
using Flensrocker.EventSourcing.Persistence.PostgreSQL;
using Flensrocker.ToDo.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using NATS.Client.Core;
using Orleans.Streaming.NATS.Hosting;
using Playground.ServiceDefaults;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

var globalJsonOptions = new EventSourcingJsonSerializerOptions().AddToDoJsonOptions();

builder.AddKeyedRedisClient(ServiceKeys.OrleansClustering);

builder.Services.AddFlensrockerEventSourcingCore();
builder.Services.AddPostgreSQLDomainMessageSubscriber(() =>
{
  return new PostgreSQLMessageSubscriberOptions { ConnectionName = ServiceKeys.PostgresDb, JsonOptions = globalJsonOptions };
});

builder.UseOrleansClient(clientBuilder =>
{
  var natsConnectionString = builder.Configuration.GetConnectionString(ServiceKeys.OrleansStreaming)
    ?? throw new InvalidOperationException("NATS connection string not configured.");
  clientBuilder.AddNatsStreams(OrleansConstants.DomainEventStreamProviderName, options =>
  {
    options.StreamName = OrleansConstants.DomainEventStreamProviderName;
    options.JsonSerializerOptions = globalJsonOptions.Options;
    options.NatsClientOptions = NatsOpts.Default with { Url = natsConnectionString };
  });
});

builder.Services.AddOrleansDomainEventStreamPublisher();

builder.Build().Run();
