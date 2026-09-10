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

    public static DomainResult Create(string? title)
    {
      var errors = DomainResult.ValidationProblem.NoErrors;

      if (string.IsNullOrWhiteSpace(title))
        errors = errors.Add("title", ["The title is required."]);
      else if (title.Length > TaskConstants.TaskTitleMaxLength)
        errors = errors.Add("title", [$"The maximum length of the title is {TaskConstants.TaskTitleMaxLength}."]);

      if (errors.Count > 0)
      {
        return new DomainResult.ValidationProblem
        {
          Title = "Invalid 'Memorize' command",
          Detail = "The task could not be created.",
          Errors = errors,
        };
      }

      System.Diagnostics.Debug.Assert(title is not null);
      return new DomainResult.Success<Memorize> { Value = new Memorize { Title = title } };
    }
  }
}
