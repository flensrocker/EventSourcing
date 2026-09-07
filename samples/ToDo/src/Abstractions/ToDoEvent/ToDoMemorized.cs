using System.Text.Json.Serialization;

namespace Flensrocker.ToDo.Abstractions;

[JsonDerivedType(typeof(ToDoMemorized), ToDoConstants.ToDoEvent.ToDoMemorized)]
public abstract partial record ToDoEvent
{
  [GenerateSerializer]
  [Alias(ToDoConstants.ToDoEvent.ToDoMemorized)]
  public sealed record ToDoMemorized : ToDoEvent
  {
    [Id(0)]
    public required DateTimeOffset CreatedAt { get; init; }

    [Id(1)]
    public required string Title { get; init; }
  }
}
