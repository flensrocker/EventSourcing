using Flensrocker.EventSourcing.Orleans.Abstractions;
using Flensrocker.EventSourcing.Orleans.Server;
using Flensrocker.ToDo.Abstractions.Tasks;
using Flensrocker.ToDo.Domain.Tasks;
using Orleans.EventSourcing.CustomStorage;
using Orleans.Providers;

namespace Flensrocker.ToDo.Orleans.Server.Tasks;

[LogConsistencyProvider(ProviderName = OrleansConstants.LogStorageProviderName)]
public class TaskCommandHandlerGrain : CommandHandlerGrain<TaskEvent, TaskCommand, TaskCommandState>
{
  public TaskCommandHandlerGrain() { }
}
