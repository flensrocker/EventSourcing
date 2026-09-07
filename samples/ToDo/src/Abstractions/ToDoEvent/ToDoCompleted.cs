using System.Text.Json.Serialization;

namespace Flensrocker.ToDo.Abstractions;

[JsonDerivedType(typeof(ToDoCompleted), ToDoConstants.ToDoEvent.ToDoCompleted)]
public abstract partial record ToDoEvent
{
  [GenerateSerializer]
  [Alias(ToDoConstants.ToDoEvent.ToDoCompleted)]
  public sealed record ToDoCompleted : ToDoEvent { }
}
