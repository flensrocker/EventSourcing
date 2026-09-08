namespace Flensrocker.ToDo.Abstractions;

public static class ToDoConstants
{
  // These constant strings aka "type names" are stored in the event store or are used by the Orleans Serializer.
  // Their values must not be changed (without a migration).
  public static class ToDoEvent
  {
    public const string Base = "ToDo_ToDoEvent";
    public const string Completed = $"{Base}_Completed";
    public const string Dismissed = $"{Base}_Dismissed";
    public const string Memorized = $"{Base}_Memorized";
    public const string Reopened = $"{Base}_Reopened";
  }

  public static class ToDoCommand
  {
    public const string Base = "ToDo_ToDoCommand";
    public const string Complete = $"{Base}_Complete";
    public const string Dismiss = $"{Base}_Dismiss";
    public const string Memorize = $"{Base}_Memorize";
    public const string Reopen = $"{Base}_Reopen";
  }

  public static class ToDoQueryRequest
  {
    public const string Base = "ToDo_ToDoQueryRequest";
    public const string List = $"{Base}_List";
  }

  public static class ToDoQueryResponse
  {
    public const string Base = "ToDo_ToDoQueryResponse";
    public const string List = $"{Base}_List";
    public const string List_Item = $"{List}_Item";
  }

  public static class ToDoView
  {
    public const string Base = "ToDo_ToDoView";
    public const string Details = $"{Base}_Details";
    public const string ListItem = $"{Base}_ListItem";
  }

  // End of "type names"

  public const int TitleMaxLength = 100;
}
