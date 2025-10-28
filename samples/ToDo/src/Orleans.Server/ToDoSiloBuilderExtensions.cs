using Flensrocker.Domain.Abstractions;
using Flensrocker.ToDo.Abstractions;
using Flensrocker.ToDo.Orleans.Abstractions;
using Orleans.Serialization;

namespace Flensrocker.ToDo.Orleans.Server;

public static class ToDoSiloBuilderExtensions
{
  public static ISiloBuilder AddToDo(this ISiloBuilder siloBuilder)
  {
    siloBuilder.Services.AddSerializer(serializerBuilder =>
    {
      serializerBuilder.AddJsonSerializer(isSupported: type => type.IsAssignableTo(typeof(ToDoCommand)) || type.IsAssignableTo(typeof(DomainEvent<ToDoEvent>)), ToDoJsonSerializerContext.Default.Options);
    });

    siloBuilder.AddMemoryStreams(ToDoNames.ToDoEventStreamProvider);
    siloBuilder.AddCustomStorageBasedLogConsistencyProvider(ToDoNames.ToDoEventStorageProvider);

    return siloBuilder;
  }
}
