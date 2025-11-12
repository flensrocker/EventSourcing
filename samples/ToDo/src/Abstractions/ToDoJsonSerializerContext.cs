using System.Text.Json.Serialization;
using Flensrocker.Domain.Abstractions;

namespace Flensrocker.ToDo.Abstractions;

[JsonSerializable(typeof(ToDoCommand))]
[JsonSerializable(typeof(ToDoCommand.CompleteToDo))]
[JsonSerializable(typeof(ToDoCommand.DismissToDo))]
[JsonSerializable(typeof(ToDoCommand.MemorizeToDo))]
[JsonSerializable(typeof(ToDoCommand.ReopenToDo))]
[JsonSerializable(typeof(ToDoEvent))]
[JsonSerializable(typeof(ToDoEvent.ToDoCompleted))]
[JsonSerializable(typeof(ToDoEvent.ToDoDismissed))]
[JsonSerializable(typeof(ToDoEvent.ToDoMemorized))]
[JsonSerializable(typeof(ToDoEvent.ToDoReopened))]
[JsonSerializable(typeof(DomainEvent<ToDoEvent>))]
[JsonSerializable(typeof(DomainEvent<ToDoEvent.ToDoCompleted>))]
[JsonSerializable(typeof(DomainEvent<ToDoEvent.ToDoDismissed>))]
[JsonSerializable(typeof(DomainEvent<ToDoEvent.ToDoMemorized>))]
[JsonSerializable(typeof(DomainEvent<ToDoEvent.ToDoReopened>))]
[JsonSerializable(typeof(ToDoQuery))]
[JsonSerializable(typeof(ToDoQuery.ListToDosRequest))]
[JsonSerializable(typeof(ToDoQuery.ListToDosResponse))]
[JsonSerializable(typeof(ToDoView))]
[JsonSerializable(typeof(ToDoView.ToDoDetails))]
[JsonSerializable(typeof(ToDoView.ToDoListItem))]
[JsonSourceGenerationOptions(AllowOutOfOrderMetadataProperties = true)]
public partial class ToDoJsonSerializerContext : JsonSerializerContext, ISupportJsonSerialization
{
  public bool Supports(Type type)
    => type.IsAssignableTo(typeof(ToDoCommand))
    || type.IsAssignableTo(typeof(ToDoEvent))
    || type.IsAssignableTo(typeof(DomainEvent<ToDoEvent>))
    || type.IsAssignableTo(typeof(ToDoQuery))
    || type.IsAssignableTo(typeof(ToDoView));
}
