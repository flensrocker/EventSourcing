namespace OrleansPlayground.ToDo.Domain;

public partial class ToDoAggregate
{
  public ToDoEvent.ToDoMemorized? Memorize(ToDoCommand.MemorizeToDo command)
  {
    if (Version > 0)
      return null;

    return new ToDoEvent.ToDoMemorized
    {
      Title = command.Title,
    };
  }
}
