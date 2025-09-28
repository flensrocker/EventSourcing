using OrleansPlayground.Abstractions.Domain;
using OrleansPlayground.ToDo.Domain;

namespace OrleansPlayground.ToDo.Views;

[GenerateSerializer]
public record ToDoListItemView
{
  [Id(0)]
  public Guid ToDoId { get; init; }
  [Id(1)]
  public required DateTimeOffset CreatedAt { get; init; }
  [Id(2)]
  public required string Title { get; init; }
  [Id(3)]
  public ToDoStatus Status { get; init; }
}

[GenerateSerializer]
public class ToDoListView : IDomainEventProjection<ToDoEvent>
{
  [Id(0)]
  public required Guid TenantId { get; init; }
  [Id(1)]
  public Dictionary<Guid, ToDoListItemView> ToDoItems { get; private set; } = new();

  public ToDoListView() { }

  private void Apply(DomainEventMetadata metadata, ToDoEvent.ToDoMemorized payload)
  {
    ToDoItems.TryAdd(metadata.EntityId, new ToDoListItemView
    {
      ToDoId = metadata.EntityId,
      CreatedAt = payload.CreatedAt,
      Title = payload.Title,
      Status = ToDoStatus.Open,
    });
  }

  private void Apply(DomainEventMetadata metadata, ToDoEvent.ToDoCompleted payload)
  {
    if (ToDoItems.TryGetValue(metadata.EntityId, out var todoItem))
    {
      ToDoItems[metadata.EntityId] = todoItem with
      {
        Status = ToDoStatus.Completed,
      };
    }
  }

  private void Apply(DomainEventMetadata metadata, ToDoEvent.ToDoDismissed payload)
  {
    if (ToDoItems.TryGetValue(metadata.EntityId, out var todoItem))
    {
      ToDoItems[metadata.EntityId] = todoItem with
      {
        Status = ToDoStatus.Dismissed,
      };
    }
  }

  private void Apply(DomainEventMetadata metadata, ToDoEvent.ToDoReopened payload)
  {
    if (ToDoItems.TryGetValue(metadata.EntityId, out var todoItem))
    {
      ToDoItems[metadata.EntityId] = todoItem with
      {
        Status = ToDoStatus.Open,
      };
    }
  }

  public void Apply(DomainEvent<ToDoEvent> @event)
  {
    switch (@event.Payload)
    {
      case ToDoEvent.ToDoMemorized payload:
        Apply(@event.Metadata, payload);
        break;
      case ToDoEvent.ToDoCompleted payload:
        Apply(@event.Metadata, payload);
        break;
      case ToDoEvent.ToDoDismissed payload:
        Apply(@event.Metadata, payload);
        break;
      case ToDoEvent.ToDoReopened payload:
        Apply(@event.Metadata, payload);
        break;
    }
  }
}
