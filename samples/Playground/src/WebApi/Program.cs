using Flensrocker.EventSourcing.Abstractions;
using Flensrocker.EventSourcing.Orleans.Abstractions;
using Flensrocker.EventSourcing.Persistence.PostgreSQL;
using Flensrocker.ToDo.Abstractions;
using Flensrocker.ToDo.Persistence.PostgreSQL;
using Flensrocker.ToDo.WebApi;
using NATS.Client.Core;
using Orleans.Serialization;
using Orleans.Streaming.NATS.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var globalJsonOptions = DomainJsonSerializerOptions.Default
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

builder.Host.UseOrleansClient(clientBuilder =>
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
