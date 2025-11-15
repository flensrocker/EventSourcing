using Flensrocker.EventSourcing.Abstractions;
using Flensrocker.EventSourcing.Orleans.Abstractions;
using Flensrocker.EventSourcing.Persistence.PostgreSQL;
using Flensrocker.ToDo.Abstractions;
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

builder.Services.AddPostgreSQLDomainMessageSubscriber(() =>
{
  return new PostgreSQLDomainMessageSubscriberOptions
  {
    ConnectionName = "playground",
    JsonOptions = globalJsonOptions,
  };
});

builder.UseOrleansClient(clientBuilder =>
{
  clientBuilder.Services.AddSerializer(serializerBuilder =>
  {
    serializerBuilder.AddJsonSerializer(globalJsonOptions.Supports, globalJsonOptions);
  });

  var natsConnectionString = builder.Configuration.GetConnectionString("nats") ?? throw new InvalidOperationException("NATS connection string not configured.");
  clientBuilder.AddNatsStreams(DomainConstants.DomainEventStreamProviderName, options =>
  {
    options.StreamName = DomainConstants.DomainEventStreamProviderName;
    options.JsonSerializerOptions = globalJsonOptions;
    options.NatsClientOptions = NatsOpts.Default with
    {
      Url = natsConnectionString,
    };
  });
});

builder.Services.AddDomainEventPublisher<OrleansDomainEventStreamPublisher>();

builder.Build().Run();
