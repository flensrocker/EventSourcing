using Flensrocker.EventSourcing.Orleans.Abstractions;
using Flensrocker.EventSourcing.Orleans.Server;
using Flensrocker.ToDo.Abstractions;
using Flensrocker.ToDo.Domain;
using Orleans.EventSourcing.CustomStorage;
using Orleans.Providers;

namespace Flensrocker.ToDo.Orleans.Server;

[LogConsistencyProvider(ProviderName = OrleansConstants.LogStorageProviderName)]
public class TaskCommandHandlerGrain : CommandHandlerGrain<TaskEvent, TaskCommand, TaskCommandState>
{
  public TaskCommandHandlerGrain() { }
}
