namespace Flensrocker.ToDo.Abstractions.Tasks;

[GenerateSerializer]
[Alias(TaskConstants.TaskCommand.Base)]
public abstract partial record TaskCommand
{
  private TaskCommand() { }
}
