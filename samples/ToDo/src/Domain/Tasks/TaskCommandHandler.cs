using Flensrocker.EventSourcing.Abstractions;
using Flensrocker.ToDo.Abstractions.Tasks;

namespace Flensrocker.ToDo.Domain.Tasks;

internal sealed class TaskCommandHandler(TimeProvider timeProvider) : ICommandHandler<TaskEvent, TaskCommand, TaskCommandState>
{
  private readonly TimeProvider _timeProvider = timeProvider;

  public Task<DomainResult<TaskEvent[]>> ExecuteCommandAsync(TaskCommandState state, TaskCommand command)
  {
    return command switch
    {
      TaskCommand.Complete complete => Complete(state, complete),
      TaskCommand.Dismiss dismiss => Dismiss(state, dismiss),
      TaskCommand.Note note => Note(state, note),
      TaskCommand.Reopen reopen => Reopen(state, reopen),
      _ => new DomainResult.InternalError($"Unhandled command type {command.GetType().FullName}"),
    };
  }

  private static DomainResult<TaskEvent[]> Complete(TaskCommandState state, TaskCommand.Complete command)
  {
    if (state.Status == Abstractions.Tasks.TaskStatus.Completed)
      return (TaskEvent[])[];

    if (state.Version == 0)
      return new DomainResult.NotFound("The task does not exist.");
    if (state.Status != Abstractions.Tasks.TaskStatus.Open)
      return new DomainResult.BadRequest("The task is not open.");

    return (TaskEvent[])[new TaskEvent.Completed { }];
  }

  private static DomainResult<TaskEvent[]> Dismiss(TaskCommandState state, TaskCommand.Dismiss command)
  {
    if (state.Status == Abstractions.Tasks.TaskStatus.Dismissed && state.DismissReason == command.Reason)
      return (TaskEvent[])[];

    if (state.Version == 0)
      return new DomainResult.NotFound("The task does not exist.");
    if (state.Status != Abstractions.Tasks.TaskStatus.Open)
      return new DomainResult.BadRequest("The task is not open.");

    return (TaskEvent[])[new TaskEvent.Dismissed { Reason = command.Reason }];
  }

  private DomainResult<TaskEvent[]> Note(TaskCommandState state, TaskCommand.Note command)
  {
    if (state.Version > 0)
      return new DomainResult.BadRequest("A task with this id already exists.");

    return (TaskEvent[])[new TaskEvent.Noted { CreatedAt = _timeProvider.GetUtcNow(), Title = command.Title }];
  }

  private static DomainResult<TaskEvent[]> Reopen(TaskCommandState state, TaskCommand.Reopen command)
  {
    if (state.Version == 0)
      return new DomainResult.NotFound("The task does not exist.");

    if (state.Status == Abstractions.Tasks.TaskStatus.Open)
      return (TaskEvent[])[];

    return (TaskEvent[])[new TaskEvent.Reopened { }];
  }
}
