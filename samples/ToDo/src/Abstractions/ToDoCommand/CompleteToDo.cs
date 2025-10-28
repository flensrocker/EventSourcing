using System.Text.Json.Serialization;

namespace Flensrocker.ToDo.Abstractions;

public abstract partial record ToDoCommand
{
  public sealed record CompleteToDo : ToDoCommand
  {
    [JsonConstructor]
    internal CompleteToDo() { }

    public static CompleteToDo? Create()
    {
      return new();
    }
  }
}
