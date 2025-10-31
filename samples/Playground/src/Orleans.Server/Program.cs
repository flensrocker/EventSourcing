using Flensrocker.Domain.Abstractions;
using Flensrocker.Domain.Persistence.InMemory;
using Flensrocker.ToDo.Abstractions;
using Flensrocker.ToDo.Orleans.Server;
using Microsoft.Extensions.Hosting;
using Orleans.Serialization;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

var globalJsonOptions = DomainJsonSerializerOptions.Default
  .AddToDoJsonOptions();

builder.AddKeyedRedisClient("redis");

builder.Services.AddInMemoryDomainEventStorage(() => new InMemoryDomainEventStorageOptions<ToDoEvent>
{
  JsonOptions = globalJsonOptions.Options,
});

builder.UseOrleans(siloBuilder =>
{
  siloBuilder.Services.AddSerializer(serializerBuilder =>
  {
    serializerBuilder.AddJsonSerializer(globalJsonOptions.Supports, globalJsonOptions.Options);
  });

  siloBuilder.UseRedisClustering("redis");
  siloBuilder.AddToDo();
});

await builder.Build().RunAsync();
