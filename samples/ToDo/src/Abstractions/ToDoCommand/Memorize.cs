using System.Text.Json.Serialization;

namespace Flensrocker.ToDo.Abstractions;

public abstract partial record ToDoCommand
{
  [GenerateSerializer]
  [Alias(ToDoConstants.ToDoCommand.Memorize)]
  public sealed record Memorize : ToDoCommand
  {
    [Id(0)]
    public required string Title { get; init; }

    [JsonConstructor]
    internal Memorize() { }

    public static Memorize? Create(string? title)
    {
      if (string.IsNullOrWhiteSpace(title) || title.Length > ToDoConstants.TitleMaxLength)
        return null;

      return new Memorize { Title = title };
    }
  }
}
