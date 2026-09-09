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
  void Apply(DomainEventMetadata metadata, TaskEvent.Memorized payload);
  void Apply(DomainEventMetadata metadata, TaskEvent.Reopened payload);

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
      case TaskEvent.Memorized payload:
        Apply(@event.Metadata, payload);
        break;
      case TaskEvent.Reopened payload:
        Apply(@event.Metadata, payload);
        break;
    }
  }
}
