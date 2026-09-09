using Flensrocker.EventSourcing.Orleans.EntityFrameworkCore;
using Flensrocker.ToDo.Abstractions.Tasks;
using Flensrocker.ToDo.Persistence.PostgreSQL;

namespace Flensrocker.ToDo.Orleans.Server.Tasks;

[ImplicitStreamSubscription(TaskConstants.TaskEvent.Base)]
public class TaskListItemProjectionGrain : EntityFrameworkCoreProjectionGrain<ToDoDbContext, TaskView.ListItem, TaskEvent>
{
  public TaskListItemProjectionGrain()
    : base(TaskConstants.TaskView.ListItem) { }
}
