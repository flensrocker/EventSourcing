namespace OrleansPlayground.ToDo.Domain;

public abstract partial record ToDoEvent
{
  public sealed record ToDoMemorized : ToDoEvent
  {
    public required string Title { get; init; }
  }
}
