using Flensrocker.ToDo.Abstractions;

namespace Flensrocker.ToDo.Orleans.Server;

public class ToDoDetailsViewGrain : ExplicitDomainEventProjectionGrain<ToDoEvent, ToDoView.Details>
{
  public ToDoDetailsViewGrain() { }
}
