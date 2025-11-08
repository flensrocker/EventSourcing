using Flensrocker.Domain.Abstractions;
using Flensrocker.Domain.Orleans.Abstractions;
using Flensrocker.ToDo.Abstractions;
using Flensrocker.ToDo.WebApi;
using NATS.Client.Core;
using Orleans.Serialization;
using Orleans.Streaming.NATS.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var globalJsonOptions = DomainJsonSerializerOptions.Default
  .AddToDoJsonOptions();

builder.AddKeyedRedisClient("redis");

builder.Host.UseOrleansClient(clientBuilder =>
{
  clientBuilder.Services.AddSerializer(serializerBuilder =>
  {
    serializerBuilder.AddJsonSerializer(globalJsonOptions.Supports, globalJsonOptions);
  });

  var natsConnectionString = builder.Configuration.GetConnectionString("nats") ?? throw new InvalidOperationException("NATS connection string not configured.");
  clientBuilder.AddNatsStreams(DomainConstants.StreamProviderDomainEvents, options =>
  {
    options.StreamName = DomainConstants.StreamProviderDomainEvents;
    options.JsonSerializerOptions = globalJsonOptions;
    options.NatsClientOptions = NatsOpts.Default with
    {
      Url = natsConnectionString,
    };
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

app.MapToDo();

app.Run();
