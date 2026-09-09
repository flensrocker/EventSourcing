using Flensrocker.EventSourcing.Abstractions;

namespace Flensrocker.ToDo.Abstractions;

public static class ToDoExtensions
{
  extension(EventSourcingJsonSerializerOptions jsonOptions)
  {
    public EventSourcingJsonSerializerOptions AddToDoJsonOptions() =>
      jsonOptions.Add(ToDoJsonSerializerContext.Default).Add<Tasks.TaskEvent>(Tasks.TaskConstants.TaskEvent.Base);
  }
}
