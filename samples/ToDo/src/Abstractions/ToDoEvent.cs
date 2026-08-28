namespace Flensrocker.ToDo.Abstractions;

[GenerateSerializer]
[Alias("Flensrocker.ToDo.ToDoEvent")]
public abstract partial record ToDoEvent
{
  private ToDoEvent() { }
}
