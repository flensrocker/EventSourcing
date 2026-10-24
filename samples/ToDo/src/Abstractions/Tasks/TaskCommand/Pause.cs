using System.Text.Json.Serialization;

namespace Flensrocker.ToDo.Abstractions.Tasks;

[JsonDerivedType(typeof(Pause), TaskConstants.TaskCommand.Pause)]
public abstract partial record TaskCommand
{
  [GenerateSerializer]
  [Alias(TaskConstants.TaskCommand.Pause)]
  public sealed record Pause : TaskCommand
  {
    [Id(0)]
    public DateTimeOffset? PausedAt { get; init; }

    [JsonConstructor]
    internal Pause() { }

    public static DomainResult<Pause> Create(DateTimeOffset? pausedAt)
    {
      return new Pause { PausedAt = pausedAt };
    }
  }
}
