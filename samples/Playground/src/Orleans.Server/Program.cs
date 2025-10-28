using Flensrocker.Domain.Persistence.InMemory;
using Flensrocker.ToDo.Abstractions;
using Flensrocker.ToDo.Orleans.Server;
using Microsoft.Extensions.Hosting;

await Host
  .CreateDefaultBuilder(args)
  .ConfigureServices(services =>
  {
    services.AddInMemoryDomainEventStorage(() => new InMemoryDomainEventStorageOptions<ToDoEvent>
    {
      JsonOptions = ToDoJsonSerializerContext.Default.Options,
    });
  })
  .UseOrleans(siloBuilder =>
  {
    siloBuilder.UseLocalhostClustering();
    siloBuilder.AddMemoryGrainStorage("PubSubStore");

    siloBuilder.AddToDo();
  })
  .RunConsoleAsync();
