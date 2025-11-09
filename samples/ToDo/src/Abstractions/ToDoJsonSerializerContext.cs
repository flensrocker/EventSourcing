using System.Text.Json.Serialization;
using Flensrocker.Domain.Abstractions;
using Flensrocker.ToDo.Abstractions.Views;

namespace Flensrocker.ToDo.Abstractions;

[JsonSerializable(typeof(ToDoCommand.CompleteToDo))]
[JsonSerializable(typeof(ToDoCommand.DismissToDo))]
[JsonSerializable(typeof(ToDoCommand.MemorizeToDo))]
[JsonSerializable(typeof(ToDoCommand.ReopenToDo))]
[JsonSerializable(typeof(DomainEvent<ToDoEvent>))]
[JsonSerializable(typeof(DomainEvent<ToDoEvent.ToDoCompleted>))]
[JsonSerializable(typeof(DomainEvent<ToDoEvent.ToDoDismissed>))]
[JsonSerializable(typeof(DomainEvent<ToDoEvent.ToDoMemorized>))]
[JsonSerializable(typeof(DomainEvent<ToDoEvent.ToDoReopened>))]
[JsonSerializable(typeof(ToDoDetailsView))]
[JsonSerializable(typeof(ToDoListItemView))]
[JsonSourceGenerationOptions(AllowOutOfOrderMetadataProperties = true)]
public partial class ToDoJsonSerializerContext : JsonSerializerContext, ISupportJsonSerialization
{
  public bool Supports(Type type)
    => type.IsAssignableTo(typeof(ToDoCommand))
    || type.IsAssignableTo(typeof(DomainEvent<ToDoEvent>))
    || type == typeof(ToDoDetailsView)
    || type == typeof(ToDoListItemView);
}
