namespace Flensrocker.ToDo.Abstractions;

public static class ToDoConstants
{
  // These constant strings aka "type names" are stored in the event store or are used by the Orleans Serializer.
  // Their values must not be changed (without a migration).
  public static class TaskEvent
  {
    public const string Base = "ToDo_TaskEvent";
    public const string Completed = $"{Base}_Completed";
    public const string Dismissed = $"{Base}_Dismissed";
    public const string Memorized = $"{Base}_Memorized";
    public const string Reopened = $"{Base}_Reopened";
  }

  public static class TaskCommand
  {
    public const string Base = "ToDo_TaskCommand";
    public const string Complete = $"{Base}_Complete";
    public const string Dismiss = $"{Base}_Dismiss";
    public const string Memorize = $"{Base}_Memorize";
    public const string Reopen = $"{Base}_Reopen";
  }

  public static class TaskQueryRequest
  {
    public const string Base = "ToDo_TaskQueryRequest";
    public const string List = $"{Base}_List";
  }

  public static class TaskQueryResponse
  {
    public const string Base = "ToDo_TaskQueryResponse";
    public const string List = $"{Base}_List";
    public const string List_Item = $"{List}_Item";
  }

  public static class TaskView
  {
    public const string Base = "ToDo_TaskView";
    public const string Details = $"{Base}_Details";
    public const string ListItem = $"{Base}_ListItem";
  }

  // End of "type names"

  public const int TaskTitleMaxLength = 100;
  public const int TaskDismissReasonMaxLength = 200;
}
