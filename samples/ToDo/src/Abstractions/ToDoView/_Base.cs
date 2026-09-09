namespace Flensrocker.ToDo.Abstractions;

[GenerateSerializer]
[Alias(ToDoConstants.ToDoView.Base)]
public abstract partial record ToDoView
{
  private ToDoView() { }
}
