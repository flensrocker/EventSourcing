using Flensrocker.ToDo.Orleans.Abstractions;

namespace Flensrocker.ToDo.Orleans.Server;

public static class ToDoSiloBuilderExtensions
{
  public static ISiloBuilder AddToDo(this ISiloBuilder siloBuilder)
  {
    siloBuilder.AddMemoryStreams(ToDoNames.ToDoEventStreamProvider);
    siloBuilder.AddCustomStorageBasedLogConsistencyProvider(ToDoNames.ToDoEventStorageProvider);

    return siloBuilder;
  }
}
