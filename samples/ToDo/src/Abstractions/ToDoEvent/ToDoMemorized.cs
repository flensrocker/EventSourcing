using System.Text.Json.Serialization;

namespace Flensrocker.ToDo.Abstractions;

[JsonDerivedType(typeof(ToDoMemorized), nameof(ToDoMemorized))]
public abstract partial record ToDoEvent
{
  [GenerateSerializer]
  [Alias("Flensrocker.ToDo.ToDoEvent.ToDoMemorized")]
  public sealed record ToDoMemorized : ToDoEvent
  {
    [Id(0)]
    public required DateTimeOffset CreatedAt { get; init; }
    [Id(1)]
    public required string Title { get; init; }
  }
}
