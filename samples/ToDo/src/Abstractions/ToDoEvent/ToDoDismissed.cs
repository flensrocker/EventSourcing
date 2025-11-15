using System.Text.Json.Serialization;

namespace Flensrocker.ToDo.Abstractions;

[JsonDerivedType(typeof(ToDoDismissed), nameof(ToDoDismissed))]
public abstract partial record ToDoEvent
{
  [GenerateSerializer]
  public sealed record ToDoDismissed : ToDoEvent
  {
    [Id(0)]
    public required string Reason { get; init; }
  }
}
