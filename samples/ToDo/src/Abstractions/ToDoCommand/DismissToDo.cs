using System.Text.Json.Serialization;

namespace Flensrocker.ToDo.Abstractions;

public abstract partial record ToDoCommand
{
  [GenerateSerializer]
  [Alias("Flensrocker.ToDo.ToDoCommand.DismissToDo")]
  public sealed record DismissToDo : ToDoCommand
  {
    [Id(0)]
    public required string Reason { get; init; }

    [JsonConstructor]
    internal DismissToDo() { }

    public static DismissToDo? Create(string? reason)
    {
      if (string.IsNullOrWhiteSpace(reason))
        return null;

      return new DismissToDo
      {
        Reason = reason,
      };
    }
  }
}
