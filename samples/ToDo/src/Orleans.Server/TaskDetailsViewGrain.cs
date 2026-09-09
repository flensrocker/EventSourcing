using Flensrocker.ToDo.Abstractions;

namespace Flensrocker.ToDo.Orleans.Server;

public class TaskDetailsViewGrain : ExplicitDomainEventProjectionGrain<TaskEvent, TaskView.Details>
{
  public TaskDetailsViewGrain() { }
}
