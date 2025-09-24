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
  public string Title { get; private set; } = "";
  public ToDoStatus Status { get; private set; }
  public string? DismissReason { get; private set; }

  public ToDoAggregate() { }

  private void Apply(DomainEventMetadata metadata, ToDoEvent.ToDoMemorized payload)
  {
    Title = payload.Title;
    Status = ToDoStatus.Open;
    DismissReason = null;
  }

  private void Apply(DomainEventMetadata metadata, ToDoEvent.ToDoCompleted payload)
  {
    Status = ToDoStatus.Completed;
    DismissReason = null;
  }

  private void Apply(DomainEventMetadata metadata, ToDoEvent.ToDoDismissed payload)
  {
    Status = ToDoStatus.Dismissed;
    DismissReason = payload.Reason;
  }

  private void Apply(DomainEventMetadata metadata, ToDoEvent.ToDoReopened payload)
  {
    Status = ToDoStatus.Open;
    DismissReason = null;
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

  public void ApplyEvents(params IEnumerable<DomainEvent<ToDoEvent>> events)
  {
    foreach (var @event in events)
      Apply(@event);
  }
}
