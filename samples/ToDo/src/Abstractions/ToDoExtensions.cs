using Flensrocker.EventSourcing.Abstractions;

namespace Flensrocker.ToDo.Abstractions;

public static class ToDoExtensions
{
  extension(EventSourcingJsonSerializerOptions jsonOptions)
  {
    public EventSourcingJsonSerializerOptions AddToDoJsonOptions() =>
      jsonOptions.Add(ToDoJsonSerializerContext.Default).Add<TaskEvent>(ToDoConstants.TaskEvent.Base);
  }
}
