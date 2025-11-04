using System.Text.Json;
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
builder.AddKeyedNatsClient("nats");

builder.Host.UseOrleansClient(clientBuilder =>
{
  clientBuilder.Services.AddSerializer(serializerBuilder =>
  {
    serializerBuilder.AddJsonSerializer(globalJsonOptions.Supports, globalJsonOptions.Options);
  });

  var natsConnectionString = builder.Configuration.GetConnectionString("nats") ?? throw new InvalidOperationException("NATS connection string not configured.");
  var natsUri = new Uri(natsConnectionString);
  clientBuilder.AddNatsStreams(DomainConstants.StreamProviderDomainEvents, options =>
  {
    options.StreamName = "Orleans";
    options.JsonSerializerOptions = new JsonSerializerOptions(globalJsonOptions.Options);
    options.NatsClientOptions = NatsOpts.Default with
    {
      Url = natsUri.OriginalString,
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
