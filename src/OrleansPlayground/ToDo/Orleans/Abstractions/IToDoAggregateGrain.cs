using Orleans;
using OrleansPlayground.ToDo.Domain;

namespace OrleansPlayground.ToDo.Orleans.Abstractions;

public interface IToDoAggregateGrain : IGrainWithGuidKey
{
  Task<bool> ExecuteCommand(ToDoCommand command);
}
