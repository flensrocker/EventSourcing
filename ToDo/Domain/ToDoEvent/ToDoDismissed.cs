namespace OrleansPlayground.ToDo.Domain;

public abstract partial record ToDoEvent
{
  public sealed record ToDoDismissed : ToDoEvent
  {
    public required string Reason { get; init; }
  }
}
