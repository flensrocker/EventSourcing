namespace Flensrocker.ToDo.Abstractions;

[GenerateSerializer]
[Alias(ToDoConstants.ToDoEventStreamType)]
public abstract partial record ToDoEvent
{
  private ToDoEvent() { }
}
