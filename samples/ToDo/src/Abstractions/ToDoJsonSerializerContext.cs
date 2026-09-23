using System.Text.Json.Serialization;

namespace Flensrocker.ToDo.Abstractions;

[JsonSerializable(typeof(Tasks.TaskCommand))]
[JsonSerializable(typeof(Tasks.TaskEvent))]
[JsonSerializable(typeof(Tasks.TaskQueryRequest.List), TypeInfoPropertyName = Tasks.TaskConstants.TaskQueryRequest.List)]
[JsonSerializable(typeof(Tasks.TaskQueryResponse.List), TypeInfoPropertyName = Tasks.TaskConstants.TaskQueryResponse.List)]
[JsonSerializable(typeof(Tasks.TaskView.Details))]
[JsonSerializable(typeof(Tasks.TaskView.ListItem))]
[JsonSourceGenerationOptions(AllowOutOfOrderMetadataProperties = true)]
public partial class ToDoJsonSerializerContext : JsonSerializerContext { }
