using Flensrocker.EventSourcing.Abstractions;
using Flensrocker.ToDo.Abstractions;

namespace Flensrocker.ToDo.Domain;

internal partial class ToDoCommandHandler(
  TimeProvider timeProvider)
  : ICommandHandler<ToDoEvent, ToDoCommand, ToDoCommandState>
{
  private readonly TimeProvider _timeProvider = timeProvider;

  public ValueTask<ToDoEvent[]?> ExecuteCommandAsync(ToDoCommandState state, ToDoCommand command)
  {
    return ValueTask.FromResult(command switch
    {
      ToDoCommand.MemorizeToDo memorizeToDo => Memorize(state, memorizeToDo),
      ToDoCommand.CompleteToDo completeToDo => Complete(state, completeToDo),
      ToDoCommand.DismissToDo dismissToDo => Dismiss(state, dismissToDo),
      ToDoCommand.ReopenToDo reopenToDo => Reopen(state, reopenToDo),
      _ => throw new NotImplementedException($"command type {command.GetType().FullName}"),
    });
  }

  private ToDoEvent[]? Memorize(ToDoCommandState state, ToDoCommand.MemorizeToDo command)
  {
    if (state.Version > 0)
      return null;

    return
    [
      new ToDoEvent.ToDoMemorized
      {
        CreatedAt = _timeProvider.GetUtcNow(),
        Title = command.Title,
      },
    ];
  }

  private ToDoEvent[]? Complete(ToDoCommandState state, ToDoCommand.CompleteToDo command)
  {
    if (state.Version == 0)
      return null;
    if (state.Status != ToDoStatus.Open)
      return null;

    return
    [
      new ToDoEvent.ToDoCompleted
      {
      },
    ];
  }

  private ToDoEvent[]? Dismiss(ToDoCommandState state, ToDoCommand.DismissToDo command)
  {
    if (state.Version == 0)
      return null;
    if (state.Status != ToDoStatus.Open)
      return null;

    return
    [
      new ToDoEvent.ToDoDismissed
      {
        Reason = command.Reason,
      },
    ];
  }

  private ToDoEvent[]? Reopen(ToDoCommandState state, ToDoCommand.ReopenToDo command)
  {
    if (state.Version == 0)
      return null;
    if (state.Status == ToDoStatus.Open)
      return null;

    return
    [
      new ToDoEvent.ToDoReopened
      {
      },
    ];
  }
}
