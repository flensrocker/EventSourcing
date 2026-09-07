using System.Text.Json.Serialization;

namespace Flensrocker.ToDo.Abstractions;

[JsonDerivedType(typeof(ToDoReopened), nameof(ToDoReopened))]
public abstract partial record ToDoEvent
{
  [GenerateSerializer]
  [Alias("ToDo.ToDoEvent.ToDoReopened")]
  public sealed record ToDoReopened : ToDoEvent { }
}
