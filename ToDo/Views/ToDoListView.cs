using OrleansPlayground.Abstractions.Domain;
using OrleansPlayground.ToDo.Abstractions;
using OrleansPlayground.ToDo.Domain;

namespace OrleansPlayground.ToDo.Views;

[GenerateSerializer]
public class ToDoListItemView : IToDoEventProjectable
{
  [Id(0)]
  public Guid ToDoId { get; private set; }
  [Id(1)]
  public DateTimeOffset CreatedAt { get; private set; }
  [Id(2)]
  public string Title { get; private set; } = "";
  [Id(3)]
  public ToDoStatus Status { get; private set; }

  void IToDoEventProjectable.Apply(DomainEventMetadata metadata, ToDoEvent.ToDoMemorized payload)
  {
    ToDoId = metadata.EntityId;
    CreatedAt = payload.CreatedAt;
    Title = payload.Title;
    Status = ToDoStatus.Open;
  }

  void IToDoEventProjectable.Apply(DomainEventMetadata metadata, ToDoEvent.ToDoCompleted payload)
  {
    Status = ToDoStatus.Completed;
  }

  void IToDoEventProjectable.Apply(DomainEventMetadata metadata, ToDoEvent.ToDoDismissed payload)
  {
    Status = ToDoStatus.Dismissed;
  }

  void IToDoEventProjectable.Apply(DomainEventMetadata metadata, ToDoEvent.ToDoReopened payload)
  {
    Status = ToDoStatus.Open;
  }

  public void Apply(DomainEvent<ToDoEvent> @event)
  {
    ((IToDoEventProjectable)this).ApplyToDoEvent(@event);
  }
}

[GenerateSerializer]
public class ToDoListView : IToDoEventProjectable
{
  [Id(0)]
  public required Guid TenantId { get; init; }
  [Id(1)]
  public Dictionary<Guid, ToDoListItemView> ToDoItems { get; private set; } = new();

  public void Apply(DomainEventMetadata metadata, ToDoEvent.ToDoMemorized payload)
  {
    var todoItem = new ToDoListItemView();
    ((IToDoEventProjectable)todoItem).Apply(metadata, payload);

    ToDoItems.TryAdd(metadata.EntityId, todoItem);
  }

  public void Apply(DomainEventMetadata metadata, ToDoEvent.ToDoCompleted payload)
  {
    if (ToDoItems.TryGetValue(metadata.EntityId, out var todoItem))
      ((IToDoEventProjectable)todoItem).Apply(metadata, payload);
  }

  public void Apply(DomainEventMetadata metadata, ToDoEvent.ToDoDismissed payload)
  {
    if (ToDoItems.TryGetValue(metadata.EntityId, out var todoItem))
      ((IToDoEventProjectable)todoItem).Apply(metadata, payload);
  }

  public void Apply(DomainEventMetadata metadata, ToDoEvent.ToDoReopened payload)
  {
    if (ToDoItems.TryGetValue(metadata.EntityId, out var todoItem))
      ((IToDoEventProjectable)todoItem).Apply(metadata, payload);
  }

  public void Apply(DomainEvent<ToDoEvent> @event)
  {
    ((IToDoEventProjectable)this).ApplyToDoEvent(@event);
  }
}
