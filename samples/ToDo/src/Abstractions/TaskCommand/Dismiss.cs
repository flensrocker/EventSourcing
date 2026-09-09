using System.Text.Json.Serialization;

namespace Flensrocker.ToDo.Abstractions;

public abstract partial record TaskCommand
{
  [GenerateSerializer]
  [Alias(ToDoConstants.TaskCommand.Dismiss)]
  public sealed record Dismiss : TaskCommand
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
