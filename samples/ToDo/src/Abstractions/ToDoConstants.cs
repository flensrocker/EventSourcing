namespace Flensrocker.ToDo.Abstractions;

public static class ToDoConstants
{
  // NOTE BEST PRACTICE
  // These constant strings aka "type names" are stored in the event store or are used by the Orleans Serializer.
  // Their values must not be changed (without a migration).
  public const string Module = "ToDo";
  // End of "type names"
}
