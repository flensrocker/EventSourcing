using Flensrocker.Domain.Persistence.InMemory;
using Flensrocker.ToDo.Abstractions;
using Flensrocker.ToDo.Orleans.Server;
using Flensrocker.ToDo.WebApi;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseOrleans(static siloBuilder =>
{
  siloBuilder.UseLocalhostClustering();
  siloBuilder.AddMemoryGrainStorage("PubSubStore");

  siloBuilder.AddToDo<InMemoryDomainEventStorage<ToDoEvent>>();
});

using var app = builder.Build();

app.MapToDo();

app.Run();
