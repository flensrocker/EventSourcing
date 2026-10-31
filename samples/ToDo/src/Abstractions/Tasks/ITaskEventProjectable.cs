using Flensrocker.EventSourcing.Abstractions;

namespace Flensrocker.ToDo.Abstractions.Tasks;

// NOTE BEST PRACTICE
// This is a helper interface to let the compiler find all places where you have to handle a newly added event.
// The Apply methods are best sorted by event name.
// Don't forget to handle the new event in the ApplyTaskEvent method!
public interface ITaskEventProjectable : IDomainEventProjectable
{
  void Apply(DomainEventMetadata metadata, TaskEvent.Completed payload);
  void Apply(DomainEventMetadata metadata, TaskEvent.Dismissed payload);
  void Apply(DomainEventMetadata metadata, TaskEvent.Opened payload);
  void Apply(DomainEventMetadata metadata, TaskEvent.Paused payload);
  void Apply(DomainEventMetadata metadata, TaskEvent.Reopened payload);
  void Apply(DomainEventMetadata metadata, TaskEvent.Started payload);
  void Apply(DomainEventMetadata metadata, TaskEvent.TitleChanged payload);

  void ApplyTaskEvent(DomainEvent @event)
  {
    switch (@event.Payload)
    {
      case TaskEvent.Completed payload:
        Apply(@event.Metadata, payload);
        break;
      case TaskEvent.Dismissed payload:
        Apply(@event.Metadata, payload);
        break;
      case TaskEvent.Opened payload:
        Apply(@event.Metadata, payload);
        break;
      case TaskEvent.Paused payload:
        Apply(@event.Metadata, payload);
        break;
      case TaskEvent.Reopened payload:
        Apply(@event.Metadata, payload);
        break;
      case TaskEvent.Started payload:
        Apply(@event.Metadata, payload);
        break;
      case TaskEvent.TitleChanged payload:
        Apply(@event.Metadata, payload);
        break;
    }
  }
}
