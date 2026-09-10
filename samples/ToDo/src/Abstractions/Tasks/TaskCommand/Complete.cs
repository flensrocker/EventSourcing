using System.Text.Json.Serialization;

namespace Flensrocker.ToDo.Abstractions.Tasks;

[JsonDerivedType(typeof(Complete), TaskConstants.TaskCommand.Complete)]
public abstract partial record TaskCommand
{
  [GenerateSerializer]
  [Alias(TaskConstants.TaskCommand.Complete)]
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
