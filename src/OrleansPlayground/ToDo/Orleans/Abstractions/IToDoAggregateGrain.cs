using Orleans;

namespace OrleansPlayground.ToDo.Orleans.Abstractions;

[GenerateSerializer]
public record MemorizeToDoDto
{
  [Id(0)]
  public string? Title { get; init; }
}

[GenerateSerializer]
public record DismissToDoDto
{
  [Id(0)]
  public string? Reason { get; init; }
}

public interface IToDoAggregateGrain : IGrainWithGuidKey
{
  Task<bool> Memorize(MemorizeToDoDto dto);
  Task<bool> Complete();
  Task<bool> Dismiss(DismissToDoDto dto);
  Task<bool> Reopen();
}
