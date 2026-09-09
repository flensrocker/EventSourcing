namespace Flensrocker.ToDo.Abstractions;

[GenerateSerializer]
[Alias(ToDoConstants.TaskView.Base)]
public abstract partial record TaskView
{
  private TaskView() { }
}
