using OrleansPlayground.ToDo.Abstractions;

namespace OrleansPlayground.ToDo.Domain;

public partial class ToDoAggregate
{
  public ToDoEvent.ToDoMemorized? Memorize(ToDoCommand.MemorizeToDo command)
  {
    if (Version > 0)
      return null;

    return new ToDoEvent.ToDoMemorized
    {
      CreatedAt = DateTimeOffset.UtcNow, // TODO TimeProvider?
      Title = command.Title,
    };
  }
}
