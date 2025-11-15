namespace Flensrocker.ToDo.Abstractions;

[GenerateSerializer]
public abstract partial record ToDoEvent
{
  private ToDoEvent() { }
}
