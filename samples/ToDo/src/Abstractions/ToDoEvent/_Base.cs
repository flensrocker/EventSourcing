namespace Flensrocker.ToDo.Abstractions;

[GenerateSerializer]
[Alias(ToDoConstants.ToDoEvent.Base)]
public abstract partial record ToDoEvent
{
  private ToDoEvent() { }
}
