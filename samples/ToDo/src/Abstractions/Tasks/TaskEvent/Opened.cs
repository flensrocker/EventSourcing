using System.Text.Json.Serialization;

namespace Flensrocker.ToDo.Abstractions.Tasks;

[JsonDerivedType(typeof(Opened), TaskConstants.TaskEvent.Opened)]
public abstract partial record TaskEvent
{
  [GenerateSerializer]
  [Alias(TaskConstants.TaskEvent.Opened)]
  public sealed record Opened : TaskEvent
  {
    [Id(0)]
    [JsonPropertyName("OpenedAt")]
    public required DateTimeOffset OpenedAt { get; init; }

    [Id(1)]
    [JsonPropertyName("Title")]
    public required string Title { get; init; }
  }
}
