using System.Text.Json.Serialization;
using Flensrocker.Domain.Abstractions;

namespace Flensrocker.ToDo.Abstractions;

public abstract partial record ToDoCommand
{
  private ToDoCommand() { }
}

[JsonSerializable(typeof(ToDoCommand.CompleteToDo))]
[JsonSerializable(typeof(ToDoCommand.DismissToDo))]
[JsonSerializable(typeof(ToDoCommand.MemorizeToDo))]
[JsonSerializable(typeof(ToDoCommand.ReopenToDo))]
public partial class ToDoCommandJsonSerializerContext : JsonSerializerContext, ISupportJsonSerialization
{
  public bool Supports(Type type)
    => type.IsAssignableTo(typeof(ToDoCommand));
}
