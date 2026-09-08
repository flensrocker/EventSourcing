using Flensrocker.EventSourcing.Abstractions;

namespace Flensrocker.ToDo.Abstractions;

public interface IToDoEventProjectable : IDomainEventProjectable
{
  void Apply(DomainEventMetadata metadata, ToDoEvent.Memorized payload);
  void Apply(DomainEventMetadata metadata, ToDoEvent.Completed payload);
  void Apply(DomainEventMetadata metadata, ToDoEvent.Dismissed payload);
  void Apply(DomainEventMetadata metadata, ToDoEvent.Reopened payload);

  void ApplyToDoEvent(DomainEvent @event)
  {
    switch (@event.Payload)
    {
      case ToDoEvent.Memorized payload:
        Apply(@event.Metadata, payload);
        break;
      case ToDoEvent.Completed payload:
        Apply(@event.Metadata, payload);
        break;
      case ToDoEvent.Dismissed payload:
        Apply(@event.Metadata, payload);
        break;
      case ToDoEvent.Reopened payload:
        Apply(@event.Metadata, payload);
        break;
    }
  }
}
