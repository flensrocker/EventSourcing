using Flensrocker.Domain.Abstractions;
using Flensrocker.ToDo.Abstractions;
using Flensrocker.ToDo.Orleans.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Serialization;

namespace Flensrocker.ToDo.Orleans.Server;

public static class ToDoSiloBuilderExtensions
{
  public static ISiloBuilder AddToDo<TDomainEventStorage>(this ISiloBuilder siloBuilder)
    where TDomainEventStorage : class, IDomainEventStorage<ToDoEvent>
  {
    siloBuilder.Services.AddScoped<TDomainEventStorage>();
    siloBuilder.Services.AddScoped<IDomainEventStorage<ToDoEvent>>(sp => sp.GetRequiredService<TDomainEventStorage>());
    siloBuilder.Services.AddSerializer(serializerBuilder =>
    {
      serializerBuilder.AddJsonSerializer(isSupported: type => type.IsAssignableTo(typeof(ToDoCommand)) || type.IsAssignableTo(typeof(DomainEvent<ToDoEvent>)), ToDoJsonSerializerContext.Default.Options);
    });

    siloBuilder.AddMemoryStreams(ToDoNames.ToDoEventStreamProvider);
    siloBuilder.AddCustomStorageBasedLogConsistencyProvider(ToDoNames.ToDoEventStorageProvider);

    return siloBuilder;
  }
}
