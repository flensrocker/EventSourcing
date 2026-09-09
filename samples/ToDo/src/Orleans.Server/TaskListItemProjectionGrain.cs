using Flensrocker.EventSourcing.Orleans.EntityFrameworkCore;
using Flensrocker.ToDo.Abstractions;
using Flensrocker.ToDo.Persistence.PostgreSQL;

namespace Flensrocker.ToDo.Orleans.Server;

[ImplicitStreamSubscription(ToDoConstants.TaskEvent.Base)]
public class TaskListItemProjectionGrain : EntityFrameworkCoreProjectionGrain<ToDoDbContext, TaskView.ListItem, TaskEvent>
{
  public TaskListItemProjectionGrain()
    : base(ToDoConstants.TaskView.ListItem) { }
}
