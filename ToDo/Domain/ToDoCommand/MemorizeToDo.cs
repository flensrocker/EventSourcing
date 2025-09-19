namespace OrleansPlayground.ToDo.Domain;

public abstract partial record ToDoCommand
{
  public sealed record MemorizeToDo : ToDoCommand
  {
    public required string Title { get; init; }

    public static MemorizeToDo? Create(string? title)
    {
      if (string.IsNullOrWhiteSpace(title))
        return null;

      return new MemorizeToDo
      {
        Title = title,
      };
    }
  }
}
