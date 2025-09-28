using Orleans.Serialization;
using OrleansPlayground.Abstractions.Domain;
using OrleansPlayground.Abstractions.Persistence;
using OrleansPlayground.Persistence;
using OrleansPlayground.ToDo.AspNetCore;
using OrleansPlayground.ToDo.Domain;
using OrleansPlayground.ToDo.Orleans.Abstractions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IDomainEventStorage<ToDoEvent>, InMemoryDomainEventStorage<ToDoEvent>>();

builder.Host.UseOrleans(static siloBuilder =>
{
  siloBuilder.UseLocalhostClustering();
  siloBuilder.AddMemoryGrainStorage("PubSubStore");

  siloBuilder.Services.AddSerializer(serializerBuilder =>
  {
    serializerBuilder.AddJsonSerializer(isSupported: type => type.IsAssignableTo(typeof(DomainEvent<ToDoEvent>)));
  });
  siloBuilder.AddMemoryStreams(ToDoNames.ToDoEventStreamProvider);
  siloBuilder.AddCustomStorageBasedLogConsistencyProvider(ToDoNames.ToDoEventStorageProvider);
});

using var app = builder.Build();

app.MapToDo();

app.Run();
