using System.Text.Json.Serialization;

namespace Flensrocker.ToDo.Abstractions;

[JsonDerivedType(typeof(Dismissed), ToDoConstants.ToDoEvent.Dismissed)]
public abstract partial record ToDoEvent
{
  [GenerateSerializer]
  [Alias(ToDoConstants.ToDoEvent.Dismissed)]
  public sealed record Dismissed : ToDoEvent
  {
    [Id(0)]
    public required string Reason { get; init; }
  }
}
