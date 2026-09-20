using System.Text.Json.Serialization;

namespace Flensrocker.ToDo.Abstractions.Tasks;

[JsonDerivedType(typeof(Noted), TaskConstants.TaskEvent.Noted)]
public abstract partial record TaskEvent
{
  [GenerateSerializer]
  [Alias(TaskConstants.TaskEvent.Noted)]
  public sealed record Noted : TaskEvent
  {
    [Id(0)]
    [JsonPropertyName("CreatedAt")]
    public required DateTimeOffset CreatedAt { get; init; }

    [Id(1)]
    [JsonPropertyName("Title")]
    public required string Title { get; init; }
  }
}
