using System.Text.Json.Serialization;

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
[JsonSerializable(
  typeof(ToDoQueryRequest.ListToDos),
  TypeInfoPropertyName = ToDoConstants.ToDoQueryRequest.ListToDos
)]
[JsonSerializable(
  typeof(ToDoQueryResponse.ListToDos),
  TypeInfoPropertyName = ToDoConstants.ToDoQueryResponse.ListToDos
)]
[JsonSerializable(typeof(ToDoView.ToDoDetails))]
[JsonSerializable(typeof(ToDoView.ToDoListItem))]
[JsonSourceGenerationOptions(AllowOutOfOrderMetadataProperties = true)]
public partial class ToDoJsonSerializerContext : JsonSerializerContext { }
