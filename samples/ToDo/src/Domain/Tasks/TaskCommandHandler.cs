using Flensrocker.EventSourcing.Abstractions;
using Flensrocker.ToDo.Abstractions.Tasks;

namespace Flensrocker.ToDo.Domain.Tasks;

internal sealed class TaskCommandHandler(TimeProvider timeProvider) : ICommandHandler<TaskEvent, TaskCommand, TaskCommandState>
{
  private readonly TimeProvider _timeProvider = timeProvider;

  Task<DomainResult<TaskEvent[]>> ICommandHandler<TaskEvent, TaskCommand, TaskCommandState>.ExecuteCommandAsync(
    TaskCommandState state,
    TaskCommand command
  )
  {
    return state.Status switch
    {
      Abstractions.Tasks.TaskStatus.Completed => FromCompletedTo(state, command),
      Abstractions.Tasks.TaskStatus.Dismissed => FromDismissedTo(state, command),
      Abstractions.Tasks.TaskStatus.Opened => FromOpenedTo(state, command),
      Abstractions.Tasks.TaskStatus.Paused => FromPausedTo(state, command),
      Abstractions.Tasks.TaskStatus.Started => FromStartedTo(state, command),
      _ => new DomainResult.InternalError($"Unhandled state {state.Status}"),
    };
  }

  private static DomainResult<TaskEvent[]> FromCompletedTo(TaskCommandState state, TaskCommand command)
  {
    return command switch
    {
      TaskCommand.Complete => (TaskEvent[])[],
      TaskCommand.Dismiss => new DomainResult.BadRequest("The task is not open."),
      TaskCommand.Note => new DomainResult.BadRequest("The task already exists."),
      TaskCommand.Pause => new DomainResult.BadRequest("The task is not open."),
      TaskCommand.Reopen => (TaskEvent[])[new TaskEvent.Reopened { }],
      TaskCommand.Start => new DomainResult.BadRequest("The task is not open."),
      _ => new DomainResult.InternalError($"Unhandled command type {command.GetType().FullName}"),
    };
  }

  private static DomainResult<TaskEvent[]> FromDismissedTo(TaskCommandState state, TaskCommand command)
  {
    return command switch
    {
      TaskCommand.Complete => new DomainResult.BadRequest("The task is already dismissed."),
      TaskCommand.Dismiss cmd when cmd.Reason == state.DismissReason => (TaskEvent[])[],
      TaskCommand.Dismiss => new DomainResult.BadRequest("The task is not open."),
      TaskCommand.Note => new DomainResult.BadRequest("The task already exists."),
      TaskCommand.Pause => new DomainResult.BadRequest("The task is not open."),
      TaskCommand.Reopen => (TaskEvent[])[new TaskEvent.Reopened { }],
      TaskCommand.Start => new DomainResult.BadRequest("The task is not open."),
      _ => new DomainResult.InternalError($"Unhandled command type {command.GetType().FullName}"),
    };
  }

  private DomainResult<TaskEvent[]> FromOpenedTo(TaskCommandState state, TaskCommand command)
  {
    return command switch
    {
      not TaskCommand.Note when state.Version == 0 => new DomainResult.NotFound("The task does not exist."),
      TaskCommand.Complete => (TaskEvent[])[new TaskEvent.Completed { }],
      TaskCommand.Dismiss cmd => (TaskEvent[])[new TaskEvent.Dismissed { Reason = cmd.Reason }],
      TaskCommand.Note when state.Version > 0 => new DomainResult.BadRequest("The task already exists."),
      TaskCommand.Note cmd => (TaskEvent[])[new TaskEvent.Noted { CreatedAt = _timeProvider.GetUtcNow(), Title = cmd.Title }],
      TaskCommand.Pause => new DomainResult.BadRequest("The task is not started."),
      TaskCommand.Reopen => (TaskEvent[])[],
      TaskCommand.Start cmd => (TaskEvent[])
        [new TaskEvent.Started { StartedAt = cmd.StartedAt ?? _timeProvider.GetUtcNow(), ByUserId = cmd.ByUserId }],
      _ => new DomainResult.InternalError($"Unhandled command type {command.GetType().FullName}"),
    };
  }

  private DomainResult<TaskEvent[]> FromPausedTo(TaskCommandState state, TaskCommand command)
  {
    return command switch
    {
      TaskCommand.Complete => (TaskEvent[])[new TaskEvent.Completed { }],
      TaskCommand.Dismiss cmd => (TaskEvent[])[new TaskEvent.Dismissed { Reason = cmd.Reason }],
      TaskCommand.Note => new DomainResult.BadRequest("The task already exists."),
      TaskCommand.Pause => (TaskEvent[])[],
      TaskCommand.Reopen => new DomainResult.BadRequest("The task is not closed."),
      TaskCommand.Start cmd => (TaskEvent[])
        [new TaskEvent.Started { StartedAt = cmd.StartedAt ?? _timeProvider.GetUtcNow(), ByUserId = cmd.ByUserId }],
      _ => new DomainResult.InternalError($"Unhandled command type {command.GetType().FullName}"),
    };
  }

  private DomainResult<TaskEvent[]> FromStartedTo(TaskCommandState state, TaskCommand command)
  {
    return command switch
    {
      TaskCommand.Complete => (TaskEvent[])[new TaskEvent.Completed { }],
      TaskCommand.Dismiss cmd => (TaskEvent[])[new TaskEvent.Dismissed { Reason = cmd.Reason }],
      TaskCommand.Note => new DomainResult.BadRequest("The task already exists."),
      TaskCommand.Pause cmd => (TaskEvent[])[new TaskEvent.Paused { PausedAt = cmd.PausedAt ?? _timeProvider.GetUtcNow() }],
      TaskCommand.Reopen => new DomainResult.BadRequest("The task is not closed."),
      TaskCommand.Start cmd when cmd.ByUserId == state.StartedByUserId => (TaskEvent[])[],
      TaskCommand.Start cmd => (TaskEvent[])
        [new TaskEvent.Started { StartedAt = cmd.StartedAt ?? _timeProvider.GetUtcNow(), ByUserId = cmd.ByUserId }],
      _ => new DomainResult.InternalError($"Unhandled command type {command.GetType().FullName}"),
    };
  }
}
