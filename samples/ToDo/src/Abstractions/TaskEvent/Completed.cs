using System.Text.Json.Serialization;

namespace Flensrocker.ToDo.Abstractions;

[JsonDerivedType(typeof(Completed), ToDoConstants.TaskEvent.Completed)]
public abstract partial record TaskEvent
{
  [GenerateSerializer]
  [Alias(ToDoConstants.TaskEvent.Completed)]
  public sealed record Completed : TaskEvent { }
}
