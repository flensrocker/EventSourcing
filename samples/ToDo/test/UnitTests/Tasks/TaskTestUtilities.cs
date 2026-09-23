using Flensrocker.EventSourcing.Abstractions;
using Flensrocker.ToDo.Abstractions.Tasks;

internal static class TaskTestUtilities
{
  internal static DomainEvent CreateTaskEvent(TimeProvider timeProvider, Guid? tenantId, Guid? userId, Guid taskId, int taskVersion, TaskEvent payload)
  {
    var taskCreatedAt = timeProvider.GetUtcNow();

    return DomainEvent.Create(
      streamType: TaskConstants.TaskEvent.Base,
      streamId: taskId,
      streamVersion: taskVersion,
      payload: payload,
      tenantId: tenantId,
      userId: userId,
      createdAt: taskCreatedAt
    );
  }
}
