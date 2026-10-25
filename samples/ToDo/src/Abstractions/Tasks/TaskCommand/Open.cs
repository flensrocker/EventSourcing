using System.Text.Json.Serialization;

namespace Flensrocker.ToDo.Abstractions.Tasks;

[JsonDerivedType(typeof(Open), TaskConstants.TaskCommand.Open)]
public abstract partial record TaskCommand
{
  [GenerateSerializer]
  [Alias(TaskConstants.TaskCommand.Open)]
  public sealed record Open : TaskCommand
  {
    [Id(0)]
    public required string Title { get; init; }

    [JsonConstructor]
    internal Open() { }

    private const string _errorTitle = "Invalid 'Open' command";
    private const string _errorDetail = "The task cannot be opened.";
    private static readonly char[] _invalidCharacters = ['\r', '\n'];

    // NOTE BEST PRACTICE
    // The static "Create" method in combination with the internal constructor is the anti corruption layer of the command.
    // To instantiate a command the caller has to use the "Create" method.
    // This ensures that every command is valid.
    // Alternative approach:
    // Use the validation properties from System.ComponentModel.DataAnnotations and rely on external validation like ASP.NET Core.
    // Advantage: less code, but also less control about the validation
    // Other alternatives can include Fluent.Validation and similar libraries.
    // The concrete implementation is not important for Flensrocker.EventSourcing and should fit the needs of the domain.
    public static DomainResult<Open> Create(string? title)
    {
      title = title?.Trim();

      if (string.IsNullOrWhiteSpace(title))
        return new DomainResult.ValidationProblem(_errorTitle, _errorDetail, nameof(Title), "The title is required.");
      if (title.Length > TaskConstants.TaskTitleMaxLength)
        return new DomainResult.ValidationProblem(
          _errorTitle,
          _errorDetail,
          nameof(Title),
          $"The title is too long, max. {TaskConstants.TaskTitleMaxLength} allowed."
        );
      if (title.IndexOfAny(_invalidCharacters) >= 0)
        return new DomainResult.ValidationProblem(_errorTitle, _errorDetail, nameof(Title), "The title contains invalid characters.");

      return new Open { Title = title };
    }
  }
}
