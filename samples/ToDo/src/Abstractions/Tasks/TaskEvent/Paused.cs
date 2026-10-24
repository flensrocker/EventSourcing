using System.Text.Json.Serialization;

namespace Flensrocker.ToDo.Abstractions.Tasks;

[JsonDerivedType(typeof(Paused), TaskConstants.TaskEvent.Paused)]
public abstract partial record TaskEvent
{
  [GenerateSerializer]
  [Alias(TaskConstants.TaskEvent.Paused)]
  public sealed record Paused : TaskEvent
  {
    [Id(0)]
    [JsonPropertyName("PausedAt")]
    public required DateTimeOffset PausedAt { get; init; }
  }
}
