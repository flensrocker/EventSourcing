using System.Text.Json.Serialization;

namespace OrleansPlayground.ToDo.Domain;

[JsonDerivedType(typeof(ToDoMemorized), nameof(ToDoMemorized))]
public abstract partial record ToDoEvent
{
  public sealed record ToDoMemorized : ToDoEvent
  {
    public required DateTimeOffset CreatedAt { get; init; }
    public required string Title { get; init; }
  }
}
