using Flensrocker.Domain.Abstractions;

namespace Flensrocker.ToDo.Abstractions;

public static class ToDoExtensions
{
  public static DomainJsonSerializerOptions AddToDoJsonOptions(this DomainJsonSerializerOptions jsonOptions)
  {
    return jsonOptions
      .Add(ToDoEventJsonSerializerContext.Default)
      .Add(ToDoCommandJsonSerializerContext.Default);
  }
}
