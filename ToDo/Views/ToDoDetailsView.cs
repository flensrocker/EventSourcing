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
}
