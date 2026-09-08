using System.Text.Json.Serialization;

namespace Flensrocker.ToDo.Abstractions;

public abstract partial record ToDoCommand
{
  [GenerateSerializer]
  [Alias(ToDoConstants.ToDoCommand.Complete)]
  public sealed record Complete : ToDoCommand
  {
    [JsonConstructor]
    internal Complete() { }

    public static Complete? Create()
    {
      return new();
    }
  }
}
