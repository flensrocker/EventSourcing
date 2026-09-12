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

    private const string _errorTitle = "Invalid 'Dismiss' command";
    private const string _errorDetail = "The task cannot be dismissed.";

    public static DomainResult<Dismiss> Create(string? reason)
    {
      if (string.IsNullOrWhiteSpace(reason))
        return new DomainResult.ValidationProblem(_errorTitle, _errorDetail, nameof(Reason), "The reason is required.");
      if (reason.Length > TaskConstants.TaskDismissReasonMaxLength)
        return new DomainResult.ValidationProblem(
          _errorTitle,
          _errorDetail,
          nameof(Reason),
          $"The reason is too long, max. {TaskConstants.TaskDismissReasonMaxLength} allowed."
        );

      return new Dismiss { Reason = reason };
    }
  }
}
