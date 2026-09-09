namespace Flensrocker.ToDo.Abstractions;

[GenerateSerializer]
[Alias(ToDoConstants.TaskCommand.Base)]
public abstract partial record TaskCommand
{
  private TaskCommand() { }
}
