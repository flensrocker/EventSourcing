namespace OrleansPlayground.ToDo.Domain;

public abstract partial record ToDoCommand
{
  public sealed record CompleteToDo : ToDoCommand
  {
    public static CompleteToDo? Create()
    {
      return new();
    }
  }
}
