using System.Text.Json.Serialization;

namespace Flensrocker.ToDo.Abstractions;

public abstract partial record TaskCommand
{
  [GenerateSerializer]
  [Alias(ToDoConstants.TaskCommand.Memorize)]
  public sealed record Memorize : TaskCommand
  {
    [Id(0)]
    public required string Title { get; init; }

    [JsonConstructor]
    internal Memorize() { }

    public static Memorize? Create(string? title)
    {
      if (string.IsNullOrWhiteSpace(title) || title.Length > ToDoConstants.TaskTitleMaxLength)
        return null;

      return new Memorize { Title = title };
    }
  }
}
