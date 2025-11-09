using Flensrocker.Domain.Abstractions;

namespace Flensrocker.ToDo.Abstractions.Views;

[GenerateSerializer]
public class ToDoListItemView : IDomainEventVersionable, IToDoEventProjectable
{
  [Id(0)]
  public Guid ToDoId { get; private set; }
  [Id(1)]
  public int Version { get; private set; }
  [Id(2)]
  public Guid? TenantId { get; private set; }
  [Id(3)]
  public DateTimeOffset CreatedAt { get; private set; }
  [Id(4)]
  public string Title { get; private set; } = "";
  [Id(5)]
  public ToDoStatus Status { get; private set; }

  void IToDoEventProjectable.Apply(DomainEventMetadata metadata, ToDoEvent.ToDoMemorized payload)
  {
    ToDoId = metadata.StreamId;
    Version = metadata.StreamVersion;
    TenantId = metadata.TenantId;
    CreatedAt = payload.CreatedAt;
    Title = payload.Title;
    Status = ToDoStatus.Open;
  }

  void IToDoEventProjectable.Apply(DomainEventMetadata metadata, ToDoEvent.ToDoCompleted payload)
  {
    Version = metadata.StreamVersion;
    Status = ToDoStatus.Completed;
  }

  void IToDoEventProjectable.Apply(DomainEventMetadata metadata, ToDoEvent.ToDoDismissed payload)
  {
    Version = metadata.StreamVersion;
    Status = ToDoStatus.Dismissed;
  }

  void IToDoEventProjectable.Apply(DomainEventMetadata metadata, ToDoEvent.ToDoReopened payload)
  {
    Version = metadata.StreamVersion;
    Status = ToDoStatus.Open;
  }

  public void Apply(DomainEvent<ToDoEvent> @event)
  {
    Version = @event.Metadata.StreamVersion;

    ((IToDoEventProjectable)this).ApplyToDoEvent(@event);
  }
}
