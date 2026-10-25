using System.Collections.Frozen;
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

  private static readonly FrozenDictionary<Type, string> _badRequestWhenCompleted = new Dictionary<Type, string>
  {
    { typeof(TaskCommand.Dismiss), "The task is already completed." },
    { typeof(TaskCommand.Note), "The task already exists." },
    { typeof(TaskCommand.Pause), "The task is already completed." },
    { typeof(TaskCommand.Start), "The task is already completed." },
  }.ToFrozenDictionary();

  private static DomainResult<TaskEvent[]> FromCompletedTo(TaskCommandState state, TaskCommand command)
  {
    return command switch
    {
      TaskCommand.Complete => (TaskEvent[])[],
      TaskCommand.Reopen => (TaskEvent[])[new TaskEvent.Reopened { }],
      _ when _badRequestWhenCompleted.TryGetValue(command.GetType(), out var errorMessage) => new DomainResult.BadRequest(errorMessage),
      _ => new DomainResult.InternalError($"Unhandled command type {command.GetType().FullName}"),
    };
  }

  private static readonly FrozenDictionary<Type, string> _badRequestWhenDismissed = new Dictionary<Type, string>
  {
    { typeof(TaskCommand.Complete), "The task is already dismissed." },
    { typeof(TaskCommand.Dismiss), "The task is already dismissed." },
    { typeof(TaskCommand.Note), "The task already exists." },
    { typeof(TaskCommand.Pause), "The task is already dismissed." },
    { typeof(TaskCommand.Start), "The task is already dismissed." },
  }.ToFrozenDictionary();

  private static DomainResult<TaskEvent[]> FromDismissedTo(TaskCommandState state, TaskCommand command)
  {
    return command switch
    {
      TaskCommand.Dismiss cmd when cmd.Reason == state.DismissReason => (TaskEvent[])[],
      TaskCommand.Reopen => (TaskEvent[])[new TaskEvent.Reopened { }],
      _ when _badRequestWhenDismissed.TryGetValue(command.GetType(), out var errorMessage) => new DomainResult.BadRequest(errorMessage),
      _ => new DomainResult.InternalError($"Unhandled command type {command.GetType().FullName}"),
    };
  }

  private static readonly FrozenDictionary<Type, string> _badRequestWhenOpened = new Dictionary<Type, string>
  {
    { typeof(TaskCommand.Pause), "The task is not started." },
  }.ToFrozenDictionary();

  private DomainResult<TaskEvent[]> FromOpenedTo(TaskCommandState state, TaskCommand command)
  {
    return command switch
    {
      not TaskCommand.Note when state.Version == 0 => new DomainResult.NotFound("The task does not exist."),
      TaskCommand.Note when state.Version > 0 => new DomainResult.BadRequest("The task already exists."),
      TaskCommand.Complete => (TaskEvent[])[new TaskEvent.Completed { }],
      TaskCommand.Dismiss cmd => (TaskEvent[])[new TaskEvent.Dismissed { Reason = cmd.Reason }],
      TaskCommand.Note cmd => (TaskEvent[])[new TaskEvent.Noted { CreatedAt = _timeProvider.GetUtcNow(), Title = cmd.Title }],
      TaskCommand.Reopen => (TaskEvent[])[],
      TaskCommand.Start cmd => (TaskEvent[])
        [new TaskEvent.Started { StartedAt = cmd.StartedAt ?? _timeProvider.GetUtcNow(), ByUserId = cmd.ByUserId }],
      _ when _badRequestWhenOpened.TryGetValue(command.GetType(), out var errorMessage) => new DomainResult.BadRequest(errorMessage),
      _ => new DomainResult.InternalError($"Unhandled command type {command.GetType().FullName}"),
    };
  }

  private static readonly FrozenDictionary<Type, string> _badRequestWhenPaused = new Dictionary<Type, string>
  {
    { typeof(TaskCommand.Note), "The task already exists." },
    { typeof(TaskCommand.Reopen), "The task is not closed." },
  }.ToFrozenDictionary();

  private DomainResult<TaskEvent[]> FromPausedTo(TaskCommandState state, TaskCommand command)
  {
    return command switch
    {
      TaskCommand.Complete => (TaskEvent[])[new TaskEvent.Completed { }],
      TaskCommand.Dismiss cmd => (TaskEvent[])[new TaskEvent.Dismissed { Reason = cmd.Reason }],
      TaskCommand.Pause => (TaskEvent[])[],
      TaskCommand.Start cmd => (TaskEvent[])
        [new TaskEvent.Started { StartedAt = cmd.StartedAt ?? _timeProvider.GetUtcNow(), ByUserId = cmd.ByUserId }],
      _ when _badRequestWhenPaused.TryGetValue(command.GetType(), out var errorMessage) => new DomainResult.BadRequest(errorMessage),
      _ => new DomainResult.InternalError($"Unhandled command type {command.GetType().FullName}"),
    };
  }

  private static readonly FrozenDictionary<Type, string> _badRequestWhenStarted = new Dictionary<Type, string>
  {
    { typeof(TaskCommand.Note), "The task already exists." },
    { typeof(TaskCommand.Reopen), "The task is not closed." },
  }.ToFrozenDictionary();

  private DomainResult<TaskEvent[]> FromStartedTo(TaskCommandState state, TaskCommand command)
  {
    return command switch
    {
      TaskCommand.Complete => (TaskEvent[])[new TaskEvent.Completed { }],
      TaskCommand.Dismiss cmd => (TaskEvent[])[new TaskEvent.Dismissed { Reason = cmd.Reason }],
      TaskCommand.Pause cmd => (TaskEvent[])[new TaskEvent.Paused { PausedAt = cmd.PausedAt ?? _timeProvider.GetUtcNow() }],
      TaskCommand.Start cmd when cmd.ByUserId == state.StartedByUserId => (TaskEvent[])[],
      TaskCommand.Start cmd => (TaskEvent[])
        [new TaskEvent.Started { StartedAt = cmd.StartedAt ?? _timeProvider.GetUtcNow(), ByUserId = cmd.ByUserId }],
      _ when _badRequestWhenStarted.TryGetValue(command.GetType(), out var errorMessage) => new DomainResult.BadRequest(errorMessage),
      _ => new DomainResult.InternalError($"Unhandled command type {command.GetType().FullName}"),
    };
  }
}
