using Flensrocker.EventSourcing.Abstractions;
using Flensrocker.ToDo.Abstractions;

namespace Flensrocker.ToDo.Domain;

[GenerateSerializer]
[Alias("ToDo_ToDoCommandState")]
public class ToDoCommandState : IDomainEventVersionable, IToDoEventProjectable
{
  [Id(0)]
  public int Version { get; private set; }

  [Id(1)]
  public ToDoStatus Status { get; private set; }

  void IToDoEventProjectable.Apply(DomainEventMetadata metadata, ToDoEvent.Memorized payload)
  {
    Status = ToDoStatus.Open;
  }

  void IToDoEventProjectable.Apply(DomainEventMetadata metadata, ToDoEvent.Completed payload)
  {
    Status = ToDoStatus.Completed;
  }

  void IToDoEventProjectable.Apply(DomainEventMetadata metadata, ToDoEvent.Dismissed payload)
  {
    Status = ToDoStatus.Dismissed;
  }

  void IToDoEventProjectable.Apply(DomainEventMetadata metadata, ToDoEvent.Reopened payload)
  {
    Status = ToDoStatus.Open;
  }

  public void Apply(DomainEvent @event)
  {
    Version = @event.Metadata.StreamVersion;

    ((IToDoEventProjectable)this).ApplyToDoEvent(@event);
  }
}
