using System.Text.Json.Serialization;

namespace Flensrocker.ToDo.Abstractions;

public abstract partial record TaskCommand
{
  [GenerateSerializer]
  [Alias(ToDoConstants.TaskCommand.Complete)]
  public sealed record Complete : TaskCommand
  {
    [JsonConstructor]
    internal Complete() { }

    public static Complete? Create()
    {
      return new();
    }
  }
}
