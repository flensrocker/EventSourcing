using Flensrocker.EventSourcing.Orleans.Server;
using Flensrocker.ToDo.Abstractions.Tasks;

namespace Flensrocker.ToDo.Orleans.Server.Tasks;

public class TaskViewDetailsGrain : ExplicitDomainEventProjectionGrain<TaskEvent, TaskView.Details>
{
  public TaskViewDetailsGrain() { }
}
