using Flensrocker.Domain.Persistence.InMemory;
using Flensrocker.ToDo.Abstractions;
using Flensrocker.ToDo.Orleans.Server;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.AddKeyedRedisClient("redis");

builder.Services.AddInMemoryDomainEventStorage(() => new InMemoryDomainEventStorageOptions<ToDoEvent>
{
  JsonOptions = ToDoJsonSerializerContext.Default.Options,
});

builder.UseOrleans(siloBuilder =>
{
  siloBuilder.UseRedisClustering("redis");
  siloBuilder.AddToDo();
});

await builder.Build().RunAsync();
