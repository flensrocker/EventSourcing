using OrleansPlayground.ToDo.Abstractions;

namespace OrleansPlayground.ToDo.Domain;

public partial class ToDoAggregate
{
  public ToDoEvent[]? Complete(ToDoCommand.CompleteToDo command)
  {
    if (Version == 0)
      return null;
    if (Status != ToDoStatus.Open)
      return null;

    return [new ToDoEvent.ToDoCompleted()];
  }
}
