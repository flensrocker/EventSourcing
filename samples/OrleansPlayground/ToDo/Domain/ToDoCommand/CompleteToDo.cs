using System.Text.Json.Serialization;

namespace OrleansPlayground.ToDo.Domain;

public abstract partial record ToDoCommand
{
  public sealed record CompleteToDo : ToDoCommand
  {
    [JsonConstructor]
    private CompleteToDo() { }

    public static CompleteToDo? Create()
    {
      return new();
    }
  }
}
