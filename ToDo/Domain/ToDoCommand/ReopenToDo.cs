namespace OrleansPlayground.ToDo.Domain;

public abstract partial record ToDoCommand
{
  public sealed record ReopenToDo : ToDoCommand
  {
    public static ReopenToDo? Create()
    {
      return new();
    }
  }
}
