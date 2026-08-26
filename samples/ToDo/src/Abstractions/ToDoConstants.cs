namespace Flensrocker.ToDo.Abstractions;

public static class ToDoConstants
{
  /// <summary>
  /// For easier adding implicit subscription attributes
  /// </summary>
  public const string ToDoEventStream = "Flensrocker-ToDo-Abstractions-ToDoEvent_-_Flensrocker-ToDo-Abstractions"; // typeof(ToDoEvent).GetTypeName()

  public const int TitleMaxLength = 100;
}
