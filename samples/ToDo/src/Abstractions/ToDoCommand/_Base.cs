namespace Flensrocker.ToDo.Abstractions;

[GenerateSerializer]
[Alias(ToDoConstants.ToDoCommand.Base)]
public abstract partial record ToDoCommand
{
  private ToDoCommand() { }
}
