using System.Text.Json.Serialization;

namespace Flensrocker.ToDo.Abstractions;

[JsonDerivedType(typeof(ToDoReopened), nameof(ToDoReopened))]
public abstract partial record ToDoEvent
{
  public sealed record ToDoReopened : ToDoEvent { }
}
