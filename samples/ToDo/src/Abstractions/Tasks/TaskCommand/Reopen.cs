using System.Text.Json.Serialization;

namespace Flensrocker.ToDo.Abstractions.Tasks;

[JsonDerivedType(typeof(Reopen), TaskConstants.TaskCommand.Reopen)]
public abstract partial record TaskCommand
{
  [GenerateSerializer]
  [Alias(TaskConstants.TaskCommand.Reopen)]
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
