using System.Text.Json.Serialization;

namespace Flensrocker.ToDo.Abstractions;

[JsonSerializable(typeof(ToDoCommand))]
[JsonSerializable(typeof(ToDoCommand.Complete))]
[JsonSerializable(typeof(ToDoCommand.Dismiss))]
[JsonSerializable(typeof(ToDoCommand.Memorize))]
[JsonSerializable(typeof(ToDoCommand.Reopen))]
[JsonSerializable(typeof(ToDoEvent))]
[JsonSerializable(typeof(ToDoEvent.Completed))]
[JsonSerializable(typeof(ToDoEvent.Dismissed))]
[JsonSerializable(typeof(ToDoEvent.Memorized))]
[JsonSerializable(typeof(ToDoEvent.Reopened))]
[JsonSerializable(typeof(ToDoQueryRequest.List), TypeInfoPropertyName = ToDoConstants.ToDoQueryRequest.List)]
[JsonSerializable(typeof(ToDoQueryResponse.List), TypeInfoPropertyName = ToDoConstants.ToDoQueryResponse.List)]
[JsonSerializable(typeof(ToDoView.Details))]
[JsonSerializable(typeof(ToDoView.ListItem))]
[JsonSourceGenerationOptions(AllowOutOfOrderMetadataProperties = true)]
public partial class ToDoJsonSerializerContext : JsonSerializerContext { }
