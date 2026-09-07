using System.Text.Json.Serialization;

namespace Flensrocker.ToDo.Abstractions;

public abstract partial record ToDoCommand
{
  [GenerateSerializer]
  [Alias(ToDoConstants.ToDoCommand.ReopenToDo)]
  public sealed record ReopenToDo : ToDoCommand
  {
    [JsonConstructor]
    internal ReopenToDo() { }

    public static ReopenToDo? Create()
    {
      return new();
    }
  }
}
