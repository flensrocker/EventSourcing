namespace Flensrocker.ToDo.Abstractions;

[GenerateSerializer]
[Alias("ToDo.ToDoCommand")]
public abstract partial record ToDoCommand
{
  private ToDoCommand() { }
}
