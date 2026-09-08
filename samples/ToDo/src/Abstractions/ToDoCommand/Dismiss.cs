using System.Text.Json.Serialization;

namespace Flensrocker.ToDo.Abstractions;

public abstract partial record ToDoCommand
{
  [GenerateSerializer]
  [Alias(ToDoConstants.ToDoCommand.Dismiss)]
  public sealed record Dismiss : ToDoCommand
  {
    [Id(0)]
    public required string Reason { get; init; }

    [JsonConstructor]
    internal Dismiss() { }

    public static Dismiss? Create(string? reason)
    {
      if (string.IsNullOrWhiteSpace(reason))
        return null;

      return new Dismiss { Reason = reason };
    }
  }
}
