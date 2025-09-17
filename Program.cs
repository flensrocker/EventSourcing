using OrleansPlayground.Abstractions.EntityFrameworkCore;
using OrleansPlayground.EntityFrameworkCore;
using OrleansPlayground.ToDo.AspNetCore;
using OrleansPlayground.ToDo.Domain;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IEventStore<Guid, ToDoEvent>, InMemoryEventStore<Guid, ToDoEvent>>();

builder.Host.UseOrleans(static siloBuilder =>
{
  siloBuilder.UseLocalhostClustering();
  siloBuilder.AddCustomStorageBasedLogConsistencyProvider("ToDoStorage");
});

using var app = builder.Build();

app.MapToDo();

app.Run();
