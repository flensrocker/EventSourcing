using System.Text.Json.Serialization;

namespace OrleansPlayground.ToDo.Abstractions;

[JsonDerivedType(typeof(ToDoDismissed), nameof(ToDoDismissed))]
public abstract partial record ToDoEvent
{
  public sealed record ToDoDismissed : ToDoEvent
  {
    public required string Reason { get; init; }
  }
}
