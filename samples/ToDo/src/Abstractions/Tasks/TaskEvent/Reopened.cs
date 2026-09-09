using System.Text.Json.Serialization;

namespace Flensrocker.ToDo.Abstractions.Tasks;

[JsonDerivedType(typeof(Reopened), TaskConstants.TaskEvent.Reopened)]
public abstract partial record TaskEvent
{
  [GenerateSerializer]
  [Alias(TaskConstants.TaskEvent.Reopened)]
  public sealed record Reopened : TaskEvent { }
}
