using System.Text.Json.Serialization;

namespace OrleansPlayground.ToDo.Domain;

[JsonDerivedType(typeof(ToDoReopened), nameof(ToDoReopened))]
public abstract partial record ToDoEvent
{
  public sealed record ToDoReopened : ToDoEvent { }
}
