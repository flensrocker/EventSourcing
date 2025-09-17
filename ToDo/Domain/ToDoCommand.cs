namespace OrleansPlayground.ToDo.Domain;

public abstract record ToDoCommand
{
  private ToDoCommand() { }

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

  public sealed record CompleteToDo : ToDoCommand
  {
    public static CompleteToDo? Create()
    {
      return new();
    }
  }

  public sealed record DismissToDo : ToDoCommand
  {
    public required string Reason { get; init; }

    public static DismissToDo? Create(string? reason)
    {
      if (string.IsNullOrWhiteSpace(reason))
        return null;

      return new DismissToDo
      {
        Reason = reason,
      };
    }
  }

  public sealed record ReopenToDo : ToDoCommand
  {
    public static ReopenToDo? Create()
    {
      return new();
    }
  }
}
