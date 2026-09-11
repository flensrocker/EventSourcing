using Flensrocker.EventSourcing.Abstractions;
using Flensrocker.ToDo.Abstractions.Tasks;

namespace Flensrocker.ToDo.Domain.Tasks;

internal partial class TaskCommandHandler(TimeProvider timeProvider) : ICommandHandler<TaskEvent, TaskCommand, TaskCommandState>
{
  private readonly TimeProvider _timeProvider = timeProvider;

  public Task<TaskEvent[]?> ExecuteCommandAsync(TaskCommandState state, TaskCommand command)
  {
    return Task.FromResult(
      command switch
      {
        TaskCommand.Complete completeToDo => Complete(state, completeToDo),
        TaskCommand.Dismiss dismissToDo => Dismiss(state, dismissToDo),
        TaskCommand.Memorize memorizeToDo => Memorize(state, memorizeToDo),
        TaskCommand.Reopen reopenToDo => Reopen(state, reopenToDo),
        _ => throw new NotImplementedException($"command type {command.GetType().FullName}"),
      }
    );
  }

  private TaskEvent[]? Complete(TaskCommandState state, TaskCommand.Complete command)
  {
    if (state.Version == 0)
      return null;
    if (state.Status != Abstractions.Tasks.TaskStatus.Open)
      return null;

    return [new TaskEvent.Completed { }];
  }

  private TaskEvent[]? Dismiss(TaskCommandState state, TaskCommand.Dismiss command)
  {
    if (state.Version == 0)
      return null;
    if (state.Status != Abstractions.Tasks.TaskStatus.Open)
      return null;

    return [new TaskEvent.Dismissed { Reason = command.Reason }];
  }

  private TaskEvent[]? Memorize(TaskCommandState state, TaskCommand.Memorize command)
  {
    if (state.Version > 0)
      return null;

    return [new TaskEvent.Memorized { CreatedAt = _timeProvider.GetUtcNow(), Title = command.Title }];
  }

  private TaskEvent[]? Reopen(TaskCommandState state, TaskCommand.Reopen command)
  {
    if (state.Version == 0)
      return null;
    if (state.Status == Abstractions.Tasks.TaskStatus.Open)
      return null;

    return [new TaskEvent.Reopened { }];
  }
}
