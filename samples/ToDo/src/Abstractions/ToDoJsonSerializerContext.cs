using System.Text.Json.Serialization;

namespace Flensrocker.ToDo.Abstractions;

[JsonSerializable(typeof(TaskCommand))]
[JsonSerializable(typeof(TaskCommand.Complete))]
[JsonSerializable(typeof(TaskCommand.Dismiss))]
[JsonSerializable(typeof(TaskCommand.Memorize))]
[JsonSerializable(typeof(TaskCommand.Reopen))]
[JsonSerializable(typeof(TaskEvent))]
[JsonSerializable(typeof(TaskEvent.Completed))]
[JsonSerializable(typeof(TaskEvent.Dismissed))]
[JsonSerializable(typeof(TaskEvent.Memorized))]
[JsonSerializable(typeof(TaskEvent.Reopened))]
[JsonSerializable(typeof(TaskQueryRequest.List), TypeInfoPropertyName = ToDoConstants.TaskQueryRequest.List)]
[JsonSerializable(typeof(TaskQueryResponse.List), TypeInfoPropertyName = ToDoConstants.TaskQueryResponse.List)]
[JsonSerializable(typeof(TaskView.Details))]
[JsonSerializable(typeof(TaskView.ListItem))]
[JsonSourceGenerationOptions(AllowOutOfOrderMetadataProperties = true)]
public partial class ToDoJsonSerializerContext : JsonSerializerContext { }
