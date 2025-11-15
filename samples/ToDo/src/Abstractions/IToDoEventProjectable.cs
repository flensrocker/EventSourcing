using Flensrocker.EventSourcing.Abstractions;

namespace Flensrocker.ToDo.Abstractions;

public interface IToDoEventProjectable : IDomainEventProjectable
{
  void Apply(DomainEventMetadata metadata, ToDoEvent.ToDoMemorized payload);
  void Apply(DomainEventMetadata metadata, ToDoEvent.ToDoCompleted payload);
  void Apply(DomainEventMetadata metadata, ToDoEvent.ToDoDismissed payload);
  void Apply(DomainEventMetadata metadata, ToDoEvent.ToDoReopened payload);

  void ApplyToDoEvent(DomainEvent @event)
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
