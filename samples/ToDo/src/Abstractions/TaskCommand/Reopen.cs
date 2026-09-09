using System.Text.Json.Serialization;

namespace Flensrocker.ToDo.Abstractions;

public abstract partial record TaskCommand
{
  [GenerateSerializer]
  [Alias(ToDoConstants.TaskCommand.Reopen)]
  public sealed record Reopen : TaskCommand
  {
    [JsonConstructor]
    internal Reopen() { }

    public static Reopen? Create()
    {
      return new();
    }
  }
}
