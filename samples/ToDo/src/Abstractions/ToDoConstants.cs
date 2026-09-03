namespace Flensrocker.ToDo.Abstractions;

public static class ToDoConstants
{
  // These constant strings aka "type names" are stored in the event store.
  // Their values must not be changed (without a migration).
  public const string ToDoEventStreamType = "Flensrocker.ToDo.ToDoEvent";
  public const string ToDoListItemProjectionType = "Flensrocker.ToDo.ToDoView.ToDoListItem";
  // End of "type names"

  public const int TitleMaxLength = 100;
}
