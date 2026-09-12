using System.Text.Json.Serialization;

namespace Flensrocker.ToDo.Abstractions.Tasks;

[JsonDerivedType(typeof(Memorize), TaskConstants.TaskCommand.Memorize)]
public abstract partial record TaskCommand
{
  [GenerateSerializer]
  [Alias(TaskConstants.TaskCommand.Memorize)]
  public sealed record Memorize : TaskCommand
  {
    [Id(0)]
    public required string Title { get; init; }

    [JsonConstructor]
    internal Memorize() { }

    private const string _errorTitle = "Invalid 'Memorize' command";
    private const string _errorDetail = "The task cannot be created.";

    public static DomainResult<Memorize> Create(string? title)
    {
      if (string.IsNullOrWhiteSpace(title))
        return new DomainResult.ValidationProblem(_errorTitle, _errorDetail, nameof(Title), "The title is required.");
      if (title.Length > TaskConstants.TaskTitleMaxLength)
        return new DomainResult.ValidationProblem(
          _errorTitle,
          _errorDetail,
          nameof(Title),
          $"The title is too long, max. {TaskConstants.TaskTitleMaxLength} allowed."
        );

      return new Memorize { Title = title };
    }
  }
}
