using System.Text.Json.Serialization;
using Flensrocker.Domain.Abstractions;

namespace Flensrocker.ToDo.Abstractions;

public abstract partial record ToDoEvent
{
  private ToDoEvent() { }
}

[JsonSerializable(typeof(DomainEvent<ToDoEvent>))]
[JsonSerializable(typeof(DomainEvent<ToDoEvent.ToDoCompleted>))]
[JsonSerializable(typeof(DomainEvent<ToDoEvent.ToDoDismissed>))]
[JsonSerializable(typeof(DomainEvent<ToDoEvent.ToDoMemorized>))]
[JsonSerializable(typeof(DomainEvent<ToDoEvent.ToDoReopened>))]
public partial class ToDoEventJsonSerializerContext : JsonSerializerContext, ISupportJsonSerialization
{
  public bool Supports(Type type)
    => type.IsAssignableTo(typeof(DomainEvent<ToDoEvent>));
}
