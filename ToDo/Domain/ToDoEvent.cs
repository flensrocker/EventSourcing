namespace OrleansPlayground.ToDo.Domain;

public abstract record ToDoEvent
{
  private ToDoEvent() { }

  public sealed record ToDoMemorized : ToDoEvent
  {
    public required string Title { get; init; }
  }

  public sealed record ToDoCompleted : ToDoEvent { }

  public sealed record ToDoDismissed : ToDoEvent
  {
    public required string Reason { get; init; }
  }

  public sealed record ToDoReopened : ToDoEvent { }
}
