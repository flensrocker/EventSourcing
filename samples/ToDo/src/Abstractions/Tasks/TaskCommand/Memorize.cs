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

    public static DomainResult<Memorize> Create(string? title)
    {
      var errors = new DomainResult.ValidationProblem { Title = "Invalid 'Memorize' command", Detail = "The task could not be created." };

      if (string.IsNullOrWhiteSpace(title))
        errors.AddError(nameof(Title), "The title is required.");
      else if (title.Length > TaskConstants.TaskTitleMaxLength)
        errors.AddError(nameof(Title), $"The maximum length of the title is {TaskConstants.TaskTitleMaxLength}.");

      if (errors.HasErrors)
        return errors;

      System.Diagnostics.Debug.Assert(title is not null);
      return new Memorize { Title = title };
    }
  }
}
