using System.Text.Json.Serialization;

namespace Flensrocker.ToDo.Abstractions.Tasks;

[JsonDerivedType(typeof(TitleChanged), TaskConstants.TaskEvent.TitleChanged)]
public abstract partial record TaskEvent
{
  [GenerateSerializer]
  [Alias(TaskConstants.TaskEvent.TitleChanged)]
  public sealed record TitleChanged : TaskEvent
  {
    [Id(0)]
    [JsonPropertyName("Title")]
    public required string Title { get; init; }
  }
}
