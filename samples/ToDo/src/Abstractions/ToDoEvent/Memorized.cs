using System.Text.Json.Serialization;

namespace Flensrocker.ToDo.Abstractions;

[JsonDerivedType(typeof(Memorized), ToDoConstants.ToDoEvent.Memorized)]
public abstract partial record ToDoEvent
{
  [GenerateSerializer]
  [Alias(ToDoConstants.ToDoEvent.Memorized)]
  public sealed record Memorized : ToDoEvent
  {
    [Id(0)]
    public required DateTimeOffset CreatedAt { get; init; }

    [Id(1)]
    public required string Title { get; init; }
  }
}
