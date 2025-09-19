namespace OrleansPlayground.ToDo.Domain;

public abstract partial record ToDoEvent
{
  public sealed record ToDoCompleted : ToDoEvent { }
}
