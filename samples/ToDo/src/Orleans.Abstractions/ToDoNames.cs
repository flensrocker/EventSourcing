namespace Flensrocker.ToDo.Orleans.Abstractions;

public static class ToDoNames
{
  /// <summary>
  /// For easier adding implicit subscription attributes
  /// </summary>
  public const string ToDoEventStream = "Flensrocker-ToDo-Abstractions-ToDoEvent_-_Flensrocker-ToDo-Abstractions"; // typeof(ToDoEvent).GetTypeName()
}
