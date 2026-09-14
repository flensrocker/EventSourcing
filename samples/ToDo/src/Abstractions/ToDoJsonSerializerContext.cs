using System.Text.Json.Serialization;

namespace Flensrocker.ToDo.Abstractions;

[JsonSerializable(typeof(Tasks.TaskCommand))]
[JsonSerializable(typeof(Tasks.TaskCommand.Complete))]
[JsonSerializable(typeof(Tasks.TaskCommand.Dismiss))]
[JsonSerializable(typeof(Tasks.TaskCommand.Note))]
[JsonSerializable(typeof(Tasks.TaskCommand.Reopen))]
[JsonSerializable(typeof(Tasks.TaskEvent))]
[JsonSerializable(typeof(Tasks.TaskEvent.Completed))]
[JsonSerializable(typeof(Tasks.TaskEvent.Dismissed))]
[JsonSerializable(typeof(Tasks.TaskEvent.Noted))]
[JsonSerializable(typeof(Tasks.TaskEvent.Reopened))]
[JsonSerializable(typeof(Tasks.TaskQueryRequest.List), TypeInfoPropertyName = Tasks.TaskConstants.TaskQueryRequest.List)]
[JsonSerializable(typeof(Tasks.TaskQueryResponse.List), TypeInfoPropertyName = Tasks.TaskConstants.TaskQueryResponse.List)]
[JsonSerializable(typeof(Tasks.TaskView.Details))]
[JsonSerializable(typeof(Tasks.TaskView.ListItem))]
[JsonSourceGenerationOptions(AllowOutOfOrderMetadataProperties = true)]
public partial class ToDoJsonSerializerContext : JsonSerializerContext { }
