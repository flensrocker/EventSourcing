using System.Text.Json.Serialization;

namespace Flensrocker.ToDo.Abstractions;

[JsonDerivedType(typeof(ToDoReopened), ToDoConstants.ToDoEvent.ToDoReopened)]
public abstract partial record ToDoEvent
{
  [GenerateSerializer]
  [Alias(ToDoConstants.ToDoEvent.ToDoReopened)]
  public sealed record ToDoReopened : ToDoEvent { }
}
