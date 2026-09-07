using Flensrocker.EventSourcing.Orleans.EntityFrameworkCore;
using Flensrocker.ToDo.Abstractions;
using Flensrocker.ToDo.Persistence.PostgreSQL;

namespace Flensrocker.ToDo.Orleans.Server;

[ImplicitStreamSubscription(ToDoConstants.ToDoEvent.Base)]
public class ToDoListItemProjectionGrain : EntityFrameworkCoreProjectionGrain<ToDoDbContext, ToDoView.ToDoListItem, ToDoEvent>
{
  public ToDoListItemProjectionGrain()
    : base(ToDoConstants.ToDoView.ToDoListItem)
  {
  }
}
