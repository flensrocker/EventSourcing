using Flensrocker.EventSourcing.Orleans.Server;
using Flensrocker.ToDo.Abstractions.Tasks;

namespace Flensrocker.ToDo.Orleans.Server.Tasks;

public class TaskDetailsViewGrain : ExplicitDomainEventProjectionGrain<TaskEvent, TaskView.Details>
{
  public TaskDetailsViewGrain() { }
}
