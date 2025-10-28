using Flensrocker.Orleans.Domain.Abstractions;
using Orleans.EventSourcing.CustomStorage;
using Orleans.Providers;
using OrleansPlayground.ToDo.Abstractions;
using OrleansPlayground.ToDo.Domain;
using OrleansPlayground.ToDo.Orleans.Abstractions;

namespace OrleansPlayground.ToDo.Orleans.Server;

[LogConsistencyProvider(ProviderName = ToDoNames.ToDoEventStorageProvider)]
public class ToDoAggregateGrain : DomainAggregateGrain<ToDoAggregate, ToDoCommand, ToDoEvent>
{
  public ToDoAggregateGrain()
    : base(ToDoNames.ToDoEventStreamProvider, ToDoNames.ToDoEventStream)
  {
  }
}
