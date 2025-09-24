using System.Text.Json.Serialization;

namespace OrleansPlayground.ToDo.Domain;

[JsonDerivedType(typeof(ToDoCompleted), nameof(ToDoCompleted))]
public abstract partial record ToDoEvent
{
  public sealed record ToDoCompleted : ToDoEvent { }
}
