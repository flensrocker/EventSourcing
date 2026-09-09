using System.Text.Json.Serialization;

namespace Flensrocker.ToDo.Abstractions;

[JsonDerivedType(typeof(Dismissed), ToDoConstants.TaskEvent.Dismissed)]
public abstract partial record TaskEvent
{
  [GenerateSerializer]
  [Alias(ToDoConstants.TaskEvent.Dismissed)]
  public sealed record Dismissed : TaskEvent
  {
    [Id(0)]
    public required string Reason { get; init; }
  }
}
