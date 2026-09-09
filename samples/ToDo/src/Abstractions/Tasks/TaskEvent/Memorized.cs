using System.Text.Json.Serialization;

namespace Flensrocker.ToDo.Abstractions.Tasks;

[JsonDerivedType(typeof(Memorized), TaskConstants.TaskEvent.Memorized)]
public abstract partial record TaskEvent
{
  [GenerateSerializer]
  [Alias(TaskConstants.TaskEvent.Memorized)]
  public sealed record Memorized : TaskEvent
  {
    [Id(0)]
    [JsonPropertyName("CreatedAt")]
    public required DateTimeOffset CreatedAt { get; init; }

    [Id(1)]
    [JsonPropertyName("Title")]
    public required string Title { get; init; }
  }
}
