using System.Text.Json.Serialization;

namespace Flensrocker.ToDo.Abstractions;

[JsonDerivedType(typeof(Memorized), ToDoConstants.TaskEvent.Memorized)]
public abstract partial record TaskEvent
{
  [GenerateSerializer]
  [Alias(ToDoConstants.TaskEvent.Memorized)]
  public sealed record Memorized : TaskEvent
  {
    [Id(0)]
    public required DateTimeOffset CreatedAt { get; init; }

    [Id(1)]
    public required string Title { get; init; }
  }
}
