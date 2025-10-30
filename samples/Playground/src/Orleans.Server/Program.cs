using Flensrocker.Domain.Persistence.InMemory;
using Flensrocker.ToDo.Abstractions;
using Flensrocker.ToDo.Orleans.Server;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddInMemoryDomainEventStorage(() => new InMemoryDomainEventStorageOptions<ToDoEvent>
{
  JsonOptions = ToDoJsonSerializerContext.Default.Options,
});

builder.UseOrleans(siloBuilder =>
{
  siloBuilder.UseLocalhostClustering();
  siloBuilder.AddMemoryGrainStorage("PubSubStore");

  siloBuilder.AddToDo();
});

await builder.Build().RunAsync();
