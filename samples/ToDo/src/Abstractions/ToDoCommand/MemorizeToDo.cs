using System.Text.Json.Serialization;

namespace Flensrocker.ToDo.Abstractions;

public abstract partial record ToDoCommand
{
  [GenerateSerializer]
  [Alias("Flensrocker.ToDo.ToDoCommand.MemorizeToDo")]
  public sealed record MemorizeToDo : ToDoCommand
  {
    [Id(0)]
    public required string Title { get; init; }

    [JsonConstructor]
    internal MemorizeToDo() { }

    public static MemorizeToDo? Create(string? title)
    {
      if (string.IsNullOrWhiteSpace(title) || title.Length > ToDoConstants.TitleMaxLength)
        return null;

      return new MemorizeToDo
      {
        Title = title,
      };
    }
  }
}
