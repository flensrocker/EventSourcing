using System.Text.Json.Serialization;

namespace Flensrocker.ToDo.Abstractions;

[JsonDerivedType(typeof(ToDoCompleted), nameof(ToDoCompleted))]
public abstract partial record ToDoEvent
{
  public sealed record ToDoCompleted : ToDoEvent { }
}
