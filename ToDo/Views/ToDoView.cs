using OrleansPlayground.ToDo.Domain;

namespace OrleansPlayground.ToDo.Views;

[GenerateSerializer]
public class ToDoView
{
  [Id(0)]
  public required Guid ToDoId { get; init; }
  [Id(1)]
  public required string Title { get; init; }
  [Id(2)]
  public ToDoStatus Status { get; init; }
  [Id(3)]
  public string? DismissReason { get; init; }
}
