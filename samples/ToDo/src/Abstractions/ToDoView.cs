namespace Flensrocker.ToDo.Abstractions;

[GenerateSerializer]
[Alias("ToDo.ToDoView")]
public abstract partial record ToDoView
{
  private ToDoView() { }
}
