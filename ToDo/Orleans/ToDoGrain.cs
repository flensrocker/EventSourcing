using Orleans.EventSourcing.CustomStorage;
using Orleans.Providers;
using OrleansPlayground.Abstractions.EntityFrameworkCore;
using OrleansPlayground.Abstractions.Orleans;
using OrleansPlayground.ToDo.Domain;

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
}

[LogConsistencyProvider(ProviderName = "ToDoStorage")]
public class ToDoGrain : DomainEventSourcedGrain<ToDoState, ToDoEvent>, IToDoGrain
{
  public ToDoGrain(IDomainEventStore<Guid, ToDoEvent> eventStore)
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

    RaiseDomainEvent(toDoEvent);
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

    RaiseDomainEvent(toDoEvent);
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

    RaiseDomainEvent(toDoEvent);
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

    RaiseDomainEvent(toDoEvent);
    await ConfirmEvents();

    return true;
  }
}
