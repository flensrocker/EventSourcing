using OrleansPlayground.Persistence;
using OrleansPlayground.ToDo.Abstractions;
using OrleansPlayground.ToDo.AspNetCore;
using OrleansPlayground.ToDo.Orleans.Server;

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
