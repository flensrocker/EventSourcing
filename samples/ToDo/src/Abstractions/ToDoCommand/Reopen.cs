using System.Text.Json.Serialization;

namespace Flensrocker.ToDo.Abstractions;

public abstract partial record ToDoCommand
{
  [GenerateSerializer]
  [Alias(ToDoConstants.ToDoCommand.Reopen)]
  public sealed record Reopen : ToDoCommand
  {
    [JsonConstructor]
    internal Reopen() { }

    public static Reopen? Create()
    {
      return new();
    }
  }
}
