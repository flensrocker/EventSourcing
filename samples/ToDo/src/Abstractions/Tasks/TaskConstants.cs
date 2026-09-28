namespace Flensrocker.ToDo.Abstractions.Tasks;

public static class TaskConstants
{
  // NOTE BEST PRACTICE
  // These constant strings aka "type names" are stored in the event store or are used by the Orleans Serializer.
  // Their values must not be changed (without a migration).
  public static class TaskCommand
  {
    public const string Base = $"{ToDoConstants.Module}_TaskCommand";
    public const string Complete = $"{Base}_Complete";
    public const string Dismiss = $"{Base}_Dismiss";
    public const string Note = $"{Base}_Note";
    public const string Reopen = $"{Base}_Reopen";
  }

  public static class TaskEvent
  {
    public const string Base = $"{ToDoConstants.Module}_TaskEvent";
    public const string Completed = $"{Base}_Completed";
    public const string Dismissed = $"{Base}_Dismissed";
    public const string Noted = $"{Base}_Noted";
    public const string Reopened = $"{Base}_Reopened";
  }

  public static class TaskQueryRequest
  {
    public const string Base = $"{ToDoConstants.Module}_TaskQueryRequest";
    public const string DailyStats = $"{Base}_DailyStats";
    public const string List = $"{Base}_List";
  }

  public static class TaskQueryResponse
  {
    public const string Base = $"{ToDoConstants.Module}_TaskQueryResponse";
    public const string DailyStats = $"{Base}_DailyStats";
    public const string DailyStats_Item = $"{DailyStats}_Item";
    public const string List = $"{Base}_List";
    public const string List_Item = $"{List}_Item";
  }

  public static class TaskView
  {
    public const string Base = $"{ToDoConstants.Module}_TaskView";
    public const string DailyStats = $"{Base}_DailyStats";
    public const string Details = $"{Base}_Details";
    public const string ListItem = $"{Base}_ListItem";
  }

  // End of "type names"

  public const int TaskTitleMaxLength = 100;
  public const int TaskDismissReasonMaxLength = 200;
}
