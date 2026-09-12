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

    // NOTE BEST PRACTICE
    // If your command contains no state and is immutable
    // return that one instance you ever need to avoid unneccessary allocations
    private static readonly Complete _instance = new();

    public static DomainResult<Complete> Create()
    {
      return _instance;
    }
  }
}
