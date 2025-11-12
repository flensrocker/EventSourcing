using Flensrocker.ToDo.Abstractions;
using Flensrocker.ToDo.Orleans.Abstractions;

namespace Flensrocker.ToDo.Orleans.Server;

public class ToDoDetailsViewGrain : ExplicitDomainEventProjectionGrain<ToDoEvent, ToDoView.ToDoDetails>, IToDoDetailsViewGrain
{
  public ToDoDetailsViewGrain()
  {
  }
}
