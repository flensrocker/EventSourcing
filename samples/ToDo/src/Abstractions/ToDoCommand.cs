namespace Flensrocker.ToDo.Abstractions;

[GenerateSerializer]
public abstract partial record ToDoCommand
{
  private ToDoCommand() { }
}
