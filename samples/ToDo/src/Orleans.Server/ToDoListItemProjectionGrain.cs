using Flensrocker.Domain.Orleans.EntityFrameworkCore;
using Flensrocker.ToDo.Abstractions;
using Flensrocker.ToDo.Orleans.Abstractions;
using Flensrocker.ToDo.Persistence.PostgreSQL;

namespace Flensrocker.ToDo.Orleans.Server;

[ImplicitStreamSubscription(ToDoNames.ToDoEventStream)]
public class ToDoListItemProjectionGrain : EntityFrameworkCoreProjectionGrain<ToDoDbContext, ToDoView.ToDoListItem, ToDoEvent>
{
}
