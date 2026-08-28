namespace Flensrocker.ToDo.Abstractions;

[GenerateSerializer]
[Alias("Flensrocker.ToDo.ToDoView")]
public abstract partial record ToDoView
{
  private ToDoView() { }
}
