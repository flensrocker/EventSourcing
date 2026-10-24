using System.Text.Json.Serialization;

namespace Flensrocker.ToDo.Abstractions.Tasks;

[JsonDerivedType(typeof(Started), TaskConstants.TaskEvent.Started)]
public abstract partial record TaskEvent
{
  [GenerateSerializer]
  [Alias(TaskConstants.TaskEvent.Started)]
  public sealed record Started : TaskEvent
  {
    [Id(0)]
    [JsonPropertyName("StartedAt")]
    public required DateTimeOffset StartedAt { get; init; }

    [Id(1)]
    [JsonPropertyName("ByUserId")]
    public Guid? ByUserId { get; init; }
  }
}
