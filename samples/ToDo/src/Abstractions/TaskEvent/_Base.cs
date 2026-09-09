namespace Flensrocker.ToDo.Abstractions;

[GenerateSerializer]
[Alias(ToDoConstants.TaskEvent.Base)]
public abstract partial record TaskEvent
{
  private TaskEvent() { }
}
