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

    private static readonly Reopen _instance = new();

    public static DomainResult<Reopen> Create()
    {
      return _instance;
    }
  }
}
