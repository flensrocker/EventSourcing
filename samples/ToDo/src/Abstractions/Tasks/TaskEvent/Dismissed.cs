using System.Text.Json.Serialization;

namespace Flensrocker.ToDo.Abstractions.Tasks;

[JsonDerivedType(typeof(Dismissed), TaskConstants.TaskEvent.Dismissed)]
public abstract partial record TaskEvent
{
  [GenerateSerializer]
  [Alias(TaskConstants.TaskEvent.Dismissed)]
  public sealed record Dismissed : TaskEvent
  {
    [Id(0)]
    [JsonPropertyName("Reason")]
    public required string Reason { get; init; }
  }
}
