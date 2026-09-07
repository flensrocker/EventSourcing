using System.Text.Json.Serialization;

namespace Flensrocker.ToDo.Abstractions;

[JsonDerivedType(typeof(ToDoDismissed), ToDoConstants.ToDoEvent.ToDoDismissed)]
public abstract partial record ToDoEvent
{
  [GenerateSerializer]
  [Alias(ToDoConstants.ToDoEvent.ToDoDismissed)]
  public sealed record ToDoDismissed : ToDoEvent
  {
    [Id(0)]
    public required string Reason { get; init; }
  }
}
