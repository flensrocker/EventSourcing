using Flensrocker.ToDo.Abstractions;
using Flensrocker.ToDo.Abstractions.Views;
using Flensrocker.ToDo.Orleans.Abstractions;

namespace Flensrocker.ToDo.Orleans.Server;

public class ToDoDetailsViewGrain : ExplicitDomainEventProjectionGrain<ToDoEvent, ToDoDetailsView>
{
  public ToDoDetailsViewGrain()
    : base(ToDoNames.ToDoEventStreamProvider, ToDoNames.ToDoEventStream)
  {
  }
}
