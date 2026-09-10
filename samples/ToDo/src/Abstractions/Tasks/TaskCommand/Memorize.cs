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

    public static Memorize? Create(string? title)
    {
      if (string.IsNullOrWhiteSpace(title) || title.Length > TaskConstants.TaskTitleMaxLength)
        return null;

      return new Memorize { Title = title };
    }
  }
}
