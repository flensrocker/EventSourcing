using System.Text.Json.Serialization;

namespace Flensrocker.ToDo.Abstractions.Tasks;

[JsonDerivedType(typeof(Start), TaskConstants.TaskCommand.Start)]
public abstract partial record TaskCommand
{
  [GenerateSerializer]
  [Alias(TaskConstants.TaskCommand.Start)]
  public sealed record Start : TaskCommand
  {
    [Id(0)]
    public DateTimeOffset? StartedAt { get; init; }

    [Id(1)]
    public Guid? ByUserId { get; init; }

    [JsonConstructor]
    internal Start() { }

    public static DomainResult<Start> Create(DateTimeOffset? startedAt, Guid? byUserId)
    {
      return new Start { StartedAt = startedAt, ByUserId = byUserId };
    }
  }
}
