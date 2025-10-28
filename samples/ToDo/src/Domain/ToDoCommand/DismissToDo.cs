using System.Text.Json.Serialization;

namespace Flensrocker.ToDo.Domain;

public abstract partial record ToDoCommand
{
  public sealed record DismissToDo : ToDoCommand
  {
    public required string Reason { get; init; }

    [JsonConstructor]
    private DismissToDo() { }

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
