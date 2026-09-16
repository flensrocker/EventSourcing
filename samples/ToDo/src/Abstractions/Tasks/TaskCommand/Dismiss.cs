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
    private static readonly char[] _invalidCharacters = ['\r', '\n'];

    public static DomainResult<Dismiss> Create(string? reason)
    {
      reason = reason?.Trim();

      if (string.IsNullOrWhiteSpace(reason))
        return new DomainResult.ValidationProblem(_errorTitle, _errorDetail, nameof(Reason), "The reason is required.");
      if (reason.Length > TaskConstants.TaskDismissReasonMaxLength)
        return new DomainResult.ValidationProblem(
          _errorTitle,
          _errorDetail,
          nameof(Reason),
          $"The reason is too long, max. {TaskConstants.TaskDismissReasonMaxLength} allowed."
        );
      if (reason.IndexOfAny(_invalidCharacters) >= 0)
        return new DomainResult.ValidationProblem(_errorTitle, _errorDetail, nameof(Reason), "The reason contains invalid characters.");

      return new Dismiss { Reason = reason };
    }
  }
}
