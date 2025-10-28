using Flensrocker.Domain.Abstractions;
using Orleans.Hosting;
using Orleans.Serialization;
using OrleansPlayground.ToDo.Abstractions;
using OrleansPlayground.ToDo.Domain;
using OrleansPlayground.ToDo.Orleans.Abstractions;

namespace OrleansPlayground.ToDo.Orleans.Server;

public static class ToDoSiloBuilderExtensions
{
  public static ISiloBuilder AddToDo<TDomainEventStorage>(this ISiloBuilder siloBuilder)
    where TDomainEventStorage : class, IDomainEventStorage<ToDoEvent>
  {
    siloBuilder.Services.AddScoped<TDomainEventStorage>();
    siloBuilder.Services.AddScoped<IDomainEventStorage<ToDoEvent>>(sp => sp.GetRequiredService<TDomainEventStorage>());
    siloBuilder.Services.AddSerializer(serializerBuilder =>
    {
      serializerBuilder.AddJsonSerializer(isSupported: type => type.IsAssignableTo(typeof(ToDoCommand)) || type.IsAssignableTo(typeof(DomainEvent<ToDoEvent>)));
    });

    siloBuilder.AddMemoryStreams(ToDoNames.ToDoEventStreamProvider);
    siloBuilder.AddCustomStorageBasedLogConsistencyProvider(ToDoNames.ToDoEventStorageProvider);

    return siloBuilder;
  }
}
