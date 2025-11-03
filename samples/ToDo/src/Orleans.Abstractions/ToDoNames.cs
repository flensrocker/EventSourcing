namespace Flensrocker.ToDo.Orleans.Abstractions;

public static class ToDoNames
{
  public const string ToDoEventStorageProvider = "ToDoEventStore";
  public const string ToDoEventStream = "Flensrocker.ToDo.Abstractions.ToDoEvent, Flensrocker.ToDo.Abstractions"; // typeof(ToDoEvent).GetTypeName()
  public const string ToDoListViewStream = "ToDoListView";
}
