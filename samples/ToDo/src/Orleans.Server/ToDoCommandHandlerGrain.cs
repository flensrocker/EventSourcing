using Flensrocker.EventSourcing.Orleans.Abstractions;
using Flensrocker.EventSourcing.Orleans.Server;
using Flensrocker.ToDo.Abstractions;
using Flensrocker.ToDo.Domain;
using Flensrocker.ToDo.Orleans.Abstractions;
using Orleans.EventSourcing.CustomStorage;
using Orleans.Providers;

namespace Flensrocker.ToDo.Orleans.Server;

[LogConsistencyProvider(ProviderName = OrleansConstants.LogStorageProviderName)]
public class ToDoCommandHandlerGrain : CommandHandlerGrain<ToDoEvent, ToDoCommand, ToDoCommandState>, IToDoCommandHandlerGrain
{
  public ToDoCommandHandlerGrain()
  {
  }
}
