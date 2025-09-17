namespace OrleansPlayground.ToDo.Domain;

public static class ToDoCommandHandler
{
  public static ToDoEvent.ToDoMemorized? Memorize(this ToDoState toDo, ToDoCommand.MemorizeToDo command, int version)
  {
    if (version > 0)
      return null;
  
    return new ToDoEvent.ToDoMemorized
    {
      Title = command.Title,
    };
  }

  public static ToDoEvent.ToDoCompleted? Complete(this ToDoState toDo, ToDoCommand.CompleteToDo command, int version)
  {
    if (version == 0)
      return null;
    if (toDo.Status != ToDoStatus.Open)
      return null;

    return new ToDoEvent.ToDoCompleted();
  }

  public static ToDoEvent.ToDoDismissed? Dismiss(this ToDoState toDo, ToDoCommand.DismissToDo command, int version)
  {
    if (version == 0)
      return null;
    if (toDo.Status != ToDoStatus.Open)
      return null;

    return new ToDoEvent.ToDoDismissed
    {
      Reason = command.Reason,
    };
  }

  public static ToDoEvent.ToDoReopened? Reopen(this ToDoState toDo, ToDoCommand.ReopenToDo command, int version)
  {
    if (version == 0)
      return null;
    if (toDo.Status == ToDoStatus.Open)
      return null;

    return new ToDoEvent.ToDoReopened();
  }
}
