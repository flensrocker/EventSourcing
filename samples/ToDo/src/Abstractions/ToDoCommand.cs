namespace Flensrocker.ToDo.Abstractions;

[GenerateSerializer]
[Alias("Flensrocker.ToDo.ToDoCommand")]
public abstract partial record ToDoCommand
{
  private ToDoCommand() { }
}
