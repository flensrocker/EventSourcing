using Flensrocker.EventSourcing.Abstractions;

namespace Flensrocker.ToDo.Abstractions;

public static class ToDoExtensions
{
  public static EventSourcingJsonSerializerOptions AddToDoJsonOptions(this EventSourcingJsonSerializerOptions jsonOptions)
  {
    return jsonOptions.Add(ToDoJsonSerializerContext.Default);
  }
}
