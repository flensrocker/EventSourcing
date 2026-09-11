using Flensrocker.EventSourcing.Abstractions;
using Flensrocker.ToDo.Abstractions.Tasks;

namespace Flensrocker.ToDo.Domain.Tasks;

internal partial class TaskCommandHandler(TimeProvider timeProvider) : ICommandHandler<TaskEvent, TaskCommand, TaskCommandState>
{
  private readonly TimeProvider _timeProvider = timeProvider;

  public Task<DomainResult<TaskEvent[]>> ExecuteCommandAsync(TaskCommandState state, TaskCommand command)
  {
    return Task.FromResult(
      command switch
      {
        TaskCommand.Complete complete => Complete(state, complete),
        TaskCommand.Dismiss dismiss => Dismiss(state, dismiss),
        TaskCommand.Memorize memorize => Memorize(state, memorize),
        TaskCommand.Reopen reopen => Reopen(state, reopen),
        _ => new DomainResult.InternalError($"Unhandled command type {command.GetType().FullName}"),
      }
    );
  }

  private DomainResult<TaskEvent[]> Complete(TaskCommandState state, TaskCommand.Complete command)
  {
    if (state.Version == 0)
      return new DomainResult.NotFound("The task does not exist.");
    if (state.Status != Abstractions.Tasks.TaskStatus.Open)
      return new DomainResult.BadRequest("The task is not open.");

    return (TaskEvent[])[new TaskEvent.Completed { }];
  }

  private DomainResult<TaskEvent[]> Dismiss(TaskCommandState state, TaskCommand.Dismiss command)
  {
    if (state.Version == 0)
      return new DomainResult.NotFound("The task does not exist.");
    if (state.Status != Abstractions.Tasks.TaskStatus.Open)
      return new DomainResult.BadRequest("The task is not open.");

    return (TaskEvent[])[new TaskEvent.Dismissed { Reason = command.Reason }];
  }

  private DomainResult<TaskEvent[]> Memorize(TaskCommandState state, TaskCommand.Memorize command)
  {
    if (state.Version > 0)
      return new DomainResult.BadRequest("A task with this id already exists.");

    return (TaskEvent[])[new TaskEvent.Memorized { CreatedAt = _timeProvider.GetUtcNow(), Title = command.Title }];
  }

  private DomainResult<TaskEvent[]> Reopen(TaskCommandState state, TaskCommand.Reopen command)
  {
    if (state.Version == 0)
      return new DomainResult.NotFound("The task does not exist.");
    if (state.Status == Abstractions.Tasks.TaskStatus.Open)
      return new DomainResult.BadRequest("The task is already open.");

    return (TaskEvent[])[new TaskEvent.Reopened { }];
  }
}
