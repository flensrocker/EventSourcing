using Flensrocker.Domain.Orleans.Server;
using Flensrocker.ToDo.Abstractions;
using Flensrocker.ToDo.Domain;
using Flensrocker.ToDo.Orleans.Abstractions;
using Orleans.EventSourcing.CustomStorage;
using Orleans.Providers;

namespace Flensrocker.ToDo.Orleans.Server;

[LogConsistencyProvider(ProviderName = ToDoNames.ToDoEventStorageProvider)]
public class ToDoAggregateGrain : DomainAggregateGrain<ToDoAggregate, ToDoCommand, ToDoEvent>, IToDoAggregateGrain
{
  public ToDoAggregateGrain()
  {
  }
}
