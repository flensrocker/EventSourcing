using Orleans.EventSourcing.CustomStorage;
using Orleans.Providers;
using OrleansPlayground.Abstractions.EntityFrameworkCore;
using OrleansPlayground.Abstractions.Orleans;
using OrleansPlayground.ToDo.Domain;
using OrleansPlayground.ToDo.Views;

namespace OrleansPlayground.ToDo.Orleans;

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

public interface IToDoGrain : IGrainWithGuidKey
{
  Task<bool> Memorize(MemorizeToDoDto dto);
  Task<bool> Complete();
  Task<bool> Dismiss(DismissToDoDto dto);
  Task<bool> Reopen();
  Task<ToDoView> GetToDo();
}

[LogConsistencyProvider(ProviderName = "ToDoStorage")]
public class ToDoGrain : EventSourcedGrain<ToDoState, ToDoEvent>, ICustomStorageInterface<ToDoState, ToDoEvent>, IToDoGrain
{
  public ToDoGrain(IEventStore<Guid, ToDoEvent> eventStore)
    : base(eventStore)
  {
  }

  public async Task<bool> Memorize(MemorizeToDoDto dto)
  {
    var command = ToDoCommand.MemorizeToDo.Create(dto.Title);
    if (command is null)
      return false;

    var toDoEvent = State.Memorize(command, Version);
    if (toDoEvent is null)
      return false;

    RaiseEvent(toDoEvent);
    await ConfirmEvents();

    return true;
  }

  public async Task<bool> Complete()
  {
    var command = ToDoCommand.CompleteToDo.Create();
    if (command is null)
      return false;

    var toDoEvent = State.Complete(command, Version);
    if (toDoEvent is null)
      return false;

    RaiseEvent(toDoEvent);
    await ConfirmEvents();

    return true;
  }

  public async Task<bool> Dismiss(DismissToDoDto dto)
  {
    var command = ToDoCommand.DismissToDo.Create(dto.Reason);
    if (command is null)
      return false;

    var toDoEvent = State.Dismiss(command, Version);
    if (toDoEvent is null)
      return false;

    RaiseEvent(toDoEvent);
    await ConfirmEvents();

    return true;
  }

  public async Task<bool> Reopen()
  {
    var command = ToDoCommand.ReopenToDo.Create();
    if (command is null)
      return false;

    var toDoEvent = State.Reopen(command, Version);
    if (toDoEvent is null)
      return false;

    RaiseEvent(toDoEvent);
    await ConfirmEvents();

    return true;
  }

  public Task<ToDoView> GetToDo()
  {
    return Task.FromResult(new ToDoView
    {
      ToDoId = this.GetPrimaryKey(),
      Title = State.Title,
      Status = State.Status,
      DismissReason = State.DismissReason,
    });
  }
}
