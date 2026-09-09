using System.Text.Json.Serialization;

namespace Flensrocker.ToDo.Abstractions;

[JsonDerivedType(typeof(Reopened), ToDoConstants.TaskEvent.Reopened)]
public abstract partial record TaskEvent
{
  [GenerateSerializer]
  [Alias(ToDoConstants.TaskEvent.Reopened)]
  public sealed record Reopened : TaskEvent { }
}
