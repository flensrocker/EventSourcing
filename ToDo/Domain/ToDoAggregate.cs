using OrleansPlayground.Abstractions.Domain;

namespace OrleansPlayground.ToDo.Domain;

public enum ToDoStatus
{
  Open = 0,
  Completed = 1,
  Dismissed = 2,
}

public partial class ToDoAggregate : IDomainEventSourceable<ToDoEvent>
{
  public int Version { get; private set; }
  public ToDoStatus Status { get; private set; }

  public ToDoAggregate() { }

  private void Apply(DomainEventMetadata metadata, ToDoEvent.ToDoMemorized payload)
  {
    Status = ToDoStatus.Open;
  }

  private void Apply(DomainEventMetadata metadata, ToDoEvent.ToDoCompleted payload)
  {
    Status = ToDoStatus.Completed;
  }

  private void Apply(DomainEventMetadata metadata, ToDoEvent.ToDoDismissed payload)
  {
    Status = ToDoStatus.Dismissed;
  }

  private void Apply(DomainEventMetadata metadata, ToDoEvent.ToDoReopened payload)
  {
    Status = ToDoStatus.Open;
  }

  public void Apply(DomainEvent<ToDoEvent> @event)
  {
    Version = @event.Metadata.EntityVersion;

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
