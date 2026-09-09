namespace Flensrocker.ToDo.Abstractions.Tasks;

[GenerateSerializer]
[Alias(TaskConstants.TaskView.Base)]
public abstract partial record TaskView
{
  private TaskView() { }
}
