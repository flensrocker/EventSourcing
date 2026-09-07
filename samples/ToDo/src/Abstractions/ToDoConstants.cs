namespace Flensrocker.ToDo.Abstractions;

public static class ToDoConstants
{
  // These constant strings aka "type names" are stored in the event store or are used by the Orleans Serializer.
  // Their values must not be changed (without a migration).
  public static class ToDoEvent
  {
    public const string Base = "ToDo_ToDoEvent";
    public const string ToDoCompleted = $"{Base}_ToDoCompleted";
    public const string ToDoDismissed = $"{Base}_ToDoDismissed";
    public const string ToDoMemorized = $"{Base}_ToDoMemorized";
    public const string ToDoReopened = $"{Base}_ToDoReopened";
  }

  public static class ToDoCommand
  {
    public const string Base = "ToDo_ToDoCommand";
    public const string CompleteToDo = $"{Base}_CompleteToDo";
    public const string DismissToDo = $"{Base}_DismissToDo";
    public const string MemorizeToDo = $"{Base}_MemorizeToDo";
    public const string ReopenToDo = $"{Base}_ReopenToDo";
  }

  public static class ToDoQueryRequest
  {
    public const string Base = "ToDo_ToDoQueryRequest";
    public const string ListToDos = $"{Base}_ListToDos";
  }

  public static class ToDoQueryResponse
  {
    public const string Base = "ToDo_ToDoQueryResponse";
    public const string ListToDos = $"{Base}_ListToDos";
    public const string ListToDos_ToDoItem = $"{ListToDos}_ToDoItem";
  }

  public static class ToDoView
  {
    public const string Base = "ToDo_ToDoView";
    public const string ToDoDetails = $"{Base}_ToDoDetails";
    public const string ToDoListItem = $"{Base}_ToDoListItem";
  }
  // End of "type names"

  public const int TitleMaxLength = 100;
}
