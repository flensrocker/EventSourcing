using Flensrocker.EventSourcing.Abstractions;
using Flensrocker.ToDo.Abstractions;

namespace Flensrocker.ToDo.Domain;

internal partial class ToDoCommandHandler(TimeProvider timeProvider) : ICommandHandler<ToDoEvent, ToDoCommand, ToDoCommandState>
{
  private readonly TimeProvider _timeProvider = timeProvider;

  public ValueTask<ToDoEvent[]?> ExecuteCommandAsync(ToDoCommandState state, ToDoCommand command)
  {
    return ValueTask.FromResult(
      command switch
      {
        ToDoCommand.Complete completeToDo => Complete(state, completeToDo),
        ToDoCommand.Dismiss dismissToDo => Dismiss(state, dismissToDo),
        ToDoCommand.Memorize memorizeToDo => Memorize(state, memorizeToDo),
        ToDoCommand.Reopen reopenToDo => Reopen(state, reopenToDo),
        _ => throw new NotImplementedException($"command type {command.GetType().FullName}"),
      }
    );
  }

  private ToDoEvent[]? Complete(ToDoCommandState state, ToDoCommand.Complete command)
  {
    if (state.Version == 0)
      return null;
    if (state.Status != ToDoStatus.Open)
      return null;

    return [new ToDoEvent.Completed { }];
  }

  private ToDoEvent[]? Dismiss(ToDoCommandState state, ToDoCommand.Dismiss command)
  {
    if (state.Version == 0)
      return null;
    if (state.Status != ToDoStatus.Open)
      return null;

    return [new ToDoEvent.Dismissed { Reason = command.Reason }];
  }

  private ToDoEvent[]? Memorize(ToDoCommandState state, ToDoCommand.Memorize command)
  {
    if (state.Version > 0)
      return null;

    return [new ToDoEvent.Memorized { CreatedAt = _timeProvider.GetUtcNow(), Title = command.Title }];
  }

  private ToDoEvent[]? Reopen(ToDoCommandState state, ToDoCommand.Reopen command)
  {
    if (state.Version == 0)
      return null;
    if (state.Status == ToDoStatus.Open)
      return null;

    return [new ToDoEvent.Reopened { }];
  }
}
