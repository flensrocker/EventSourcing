using System.Text.Json.Serialization;

namespace Flensrocker.ToDo.Domain;

public abstract partial record ToDoCommand
{
  public sealed record ReopenToDo : ToDoCommand
  {
    [JsonConstructor]
    private ReopenToDo() { }

    public static ReopenToDo? Create()
    {
      return new();
    }
  }
}
