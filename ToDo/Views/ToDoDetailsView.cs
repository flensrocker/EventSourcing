using OrleansPlayground.Abstractions.Domain;
using OrleansPlayground.ToDo.Domain;

namespace OrleansPlayground.ToDo.Views;

[GenerateSerializer]
public class ToDoDetailsView : IDomainEventSourceable<ToDoEvent>
{
  [Id(0)]
  public int Version { get; private set; }
  [Id(1)]
  public string Title { get; private set; } = "";
  [Id(2)]
  public ToDoStatus Status { get; private set; }
  [Id(3)]
  public string? DismissReason { get; private set; }

  public ToDoDetailsView() { }

  public void Apply(DomainEventMetadata metadata, ToDoEvent.ToDoMemorized payload)
  {
    Version++;
    Title = payload.Title;
    Status = ToDoStatus.Open;
    DismissReason = null;
  }

  public void Apply(DomainEventMetadata metadata, ToDoEvent.ToDoCompleted payload)
  {
    Version++;
    Status = ToDoStatus.Completed;
    DismissReason = null;
  }

  public void Apply(DomainEventMetadata metadata, ToDoEvent.ToDoDismissed payload)
  {
    Version++;
    Status = ToDoStatus.Dismissed;
    DismissReason = payload.Reason;
  }

  public void Apply(DomainEventMetadata metadata, ToDoEvent.ToDoReopened payload)
  {
    Version++;
    Status = ToDoStatus.Open;
    DismissReason = null;
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

  public void ApplyEvents(params IEnumerable<DomainEvent<ToDoEvent>> events)
  {
    foreach (var @event in events)
      Apply(@event);
  }
}
