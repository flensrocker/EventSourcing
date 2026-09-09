namespace Flensrocker.ToDo.Abstractions.Tasks;

// NOTE BEST PRACTICE
// All events of a domain or subdomain must be derived from an abstract base class with a private constructor.
// All implementations must be nested inside this base class.
// All implementations must be sealed.
// These best practices are also valid for commands, query requests and responses, and views.
// TODO With C# 15 (.NET 11) the "abstract" keyword can be replaced with "closed" and the private constructor can be removed
[GenerateSerializer]
[Alias(TaskConstants.TaskEvent.Base)]
public abstract partial record TaskEvent
{
  private TaskEvent() { }
}
