using System.Text.Json.Serialization;

namespace Flensrocker.ToDo.Abstractions;

[JsonSerializable(typeof(Tasks.TaskCommand))]
[JsonSerializable(typeof(Tasks.TaskEvent))]
[JsonSerializable(typeof(Tasks.TaskQueryRequest.DailyStats), TypeInfoPropertyName = Tasks.TaskConstants.TaskQueryRequest.DailyStats)]
[JsonSerializable(typeof(Tasks.TaskQueryRequest.List), TypeInfoPropertyName = Tasks.TaskConstants.TaskQueryRequest.List)]
[JsonSerializable(typeof(Tasks.TaskQueryResponse.DailyStats), TypeInfoPropertyName = Tasks.TaskConstants.TaskQueryResponse.DailyStats)]
[JsonSerializable(
  typeof(Tasks.TaskQueryResponse.DailyStats.DailyStatsItem),
  TypeInfoPropertyName = Tasks.TaskConstants.TaskQueryResponse.DailyStats_Item
)]
[JsonSerializable(typeof(Tasks.TaskQueryResponse.List), TypeInfoPropertyName = Tasks.TaskConstants.TaskQueryResponse.List)]
[JsonSerializable(typeof(Tasks.TaskQueryResponse.List.ListItem), TypeInfoPropertyName = Tasks.TaskConstants.TaskQueryResponse.List_Item)]
[JsonSerializable(typeof(Tasks.TaskView.DailyStats))]
[JsonSerializable(typeof(Tasks.TaskView.Details))]
[JsonSerializable(typeof(Tasks.TaskView.ListItem))]
[JsonSourceGenerationOptions(AllowOutOfOrderMetadataProperties = true)]
public partial class ToDoJsonSerializerContext : JsonSerializerContext { }
