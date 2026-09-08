using System.Text.Json.Serialization;

namespace Flensrocker.ToDo.Abstractions;

[JsonDerivedType(typeof(Completed), ToDoConstants.ToDoEvent.Completed)]
public abstract partial record ToDoEvent
{
  [GenerateSerializer]
  [Alias(ToDoConstants.ToDoEvent.Completed)]
  public sealed record Completed : ToDoEvent { }
}
