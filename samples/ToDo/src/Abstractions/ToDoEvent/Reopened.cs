using System.Text.Json.Serialization;

namespace Flensrocker.ToDo.Abstractions;

[JsonDerivedType(typeof(Reopened), ToDoConstants.ToDoEvent.Reopened)]
public abstract partial record ToDoEvent
{
  [GenerateSerializer]
  [Alias(ToDoConstants.ToDoEvent.Reopened)]
  public sealed record Reopened : ToDoEvent { }
}
