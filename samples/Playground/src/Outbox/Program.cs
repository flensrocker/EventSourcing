using Flensrocker.EventSourcing.Abstractions;
using Flensrocker.EventSourcing.Orleans.Abstractions;
using Flensrocker.EventSourcing.Persistence.PostgreSQL;
using Flensrocker.ToDo.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using NATS.Client.Core;
using Orleans.Streaming.NATS.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

var globalJsonOptions = EventSourcingJsonSerializerOptions.Default
  .AddToDoJsonOptions();

builder.AddKeyedRedisClient("redis");

builder.Services.AddPostgreSQLDomainMessageSubscriber(() =>
{
  return new PostgreSQLMessageSubscriberOptions
  {
    ConnectionName = "playground",
    JsonOptions = globalJsonOptions,
  };
});

builder.UseOrleansClient(clientBuilder =>
{
  var natsConnectionString = builder.Configuration.GetConnectionString("nats") ?? throw new InvalidOperationException("NATS connection string not configured.");
  clientBuilder.AddNatsStreams(OrleansConstants.DomainEventStreamProviderName, options =>
  {
    options.StreamName = OrleansConstants.DomainEventStreamProviderName;
    options.JsonSerializerOptions = globalJsonOptions;
    options.NatsClientOptions = NatsOpts.Default with
    {
      Url = natsConnectionString,
    };
  });
});

builder.Services.AddDomainEventPublisher<OrleansDomainEventStreamPublisher>();

builder.Build().Run();
