using OrleansPlayground.Abstractions.EntityFrameworkCore;
using OrleansPlayground.EntityFrameworkCore;
using OrleansPlayground.ToDo.AspNetCore;
using OrleansPlayground.ToDo.Domain;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IDomainEventStore<Guid, ToDoEvent>, InMemoryDomainEventStore<Guid, ToDoEvent>>();

builder.Host.UseOrleans(static siloBuilder =>
{
  siloBuilder.UseLocalhostClustering();
  siloBuilder
    .AddMemoryStreams("StreamProvider")
    .AddMemoryGrainStorage("PubSubStore");
  siloBuilder.AddCustomStorageBasedLogConsistencyProvider("ToDoStorage");
});

using var app = builder.Build();

app.MapToDo();

app.Run();
