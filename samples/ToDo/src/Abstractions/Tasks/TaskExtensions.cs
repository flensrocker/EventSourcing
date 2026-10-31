namespace Flensrocker.ToDo.Abstractions.Tasks;

public static class TaskExtensions
{
  private static readonly char[] _invalidCharacters = ['\r', '\n'];

  public static DomainResult<string> ValidateTitle(string? title, string errorTitle, string errorDetail, string errorKey)
  {
    title = title?.Trim();

    if (string.IsNullOrWhiteSpace(title))
      return new DomainResult.ValidationProblem(errorTitle, errorDetail, errorKey, "The title is required.");
    if (title.Length > TaskConstants.TaskTitleMaxLength)
      return new DomainResult.ValidationProblem(
        errorTitle,
        errorDetail,
        errorKey,
        $"The title is too long, max. {TaskConstants.TaskTitleMaxLength} allowed."
      );
    if (title.IndexOfAny(_invalidCharacters) >= 0)
      return new DomainResult.ValidationProblem(errorTitle, errorDetail, errorKey, "The title contains invalid characters.");

    return title;
  }
}
