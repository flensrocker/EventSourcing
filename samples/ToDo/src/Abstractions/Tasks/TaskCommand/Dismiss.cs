using System.Text.Json.Serialization;

namespace Flensrocker.ToDo.Abstractions.Tasks;

[JsonDerivedType(typeof(Dismiss), TaskConstants.TaskCommand.Dismiss)]
public abstract partial record TaskCommand
{
  [GenerateSerializer]
  [Alias(TaskConstants.TaskCommand.Dismiss)]
  public sealed record Dismiss : TaskCommand
  {
    [Id(0)]
    public required string Reason { get; init; }

    [JsonConstructor]
    internal Dismiss() { }

    public static Dismiss? Create(string? reason)
    {
      if (string.IsNullOrWhiteSpace(reason) || reason.Length > TaskConstants.TaskDismissReasonMaxLength)
        return null;

      return new Dismiss { Reason = reason };
    }
  }
}
