using System.Text.Json.Serialization;
using Flensrocker.Domain.Abstractions;

namespace Flensrocker.ToDo.Abstractions;

[JsonSerializable(typeof(DomainEvent<ToDoEvent>))]
[JsonSerializable(typeof(DomainEvent<ToDoEvent.ToDoCompleted>))]
[JsonSerializable(typeof(DomainEvent<ToDoEvent.ToDoDismissed>))]
[JsonSerializable(typeof(DomainEvent<ToDoEvent.ToDoMemorized>))]
[JsonSerializable(typeof(DomainEvent<ToDoEvent.ToDoReopened>))]
[JsonSerializable(typeof(ToDoCommand.CompleteToDo))]
[JsonSerializable(typeof(ToDoCommand.DismissToDo))]
[JsonSerializable(typeof(ToDoCommand.MemorizeToDo))]
[JsonSerializable(typeof(ToDoCommand.ReopenToDo))]
[JsonSourceGenerationOptions(AllowOutOfOrderMetadataProperties = true, PropertyNamingPolicy = JsonKnownNamingPolicy.Unspecified)]
public partial class ToDoJsonSerializerContext : JsonSerializerContext
{
}
