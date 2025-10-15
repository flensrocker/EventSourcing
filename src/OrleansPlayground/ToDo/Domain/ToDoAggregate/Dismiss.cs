using OrleansPlayground.ToDo.Abstractions;

namespace OrleansPlayground.ToDo.Domain;

public partial class ToDoAggregate
{
  public ToDoEvent.ToDoDismissed? Dismiss(ToDoCommand.DismissToDo command)
  {
    if (Version == 0)
      return null;
    if (Status != ToDoStatus.Open)
      return null;

    return new ToDoEvent.ToDoDismissed
    {
      Reason = command.Reason,
    };
  }
}
