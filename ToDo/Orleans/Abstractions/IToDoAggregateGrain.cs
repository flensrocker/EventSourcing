using OrleansPlayground.ToDo.Orleans.Server;

namespace OrleansPlayground.ToDo.Orleans.Abstractions;

public interface IToDoAggregateGrain : IGrainWithGuidKey
{
  Task<bool> Memorize(MemorizeToDoDto dto);
  Task<bool> Complete();
  Task<bool> Dismiss(DismissToDoDto dto);
  Task<bool> Reopen();
}
