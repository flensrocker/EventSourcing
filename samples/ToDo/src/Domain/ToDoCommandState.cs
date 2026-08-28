using Flensrocker.EventSourcing.Abstractions;
using Flensrocker.ToDo.Abstractions;

namespace Flensrocker.ToDo.Domain;

[GenerateSerializer]
[Alias("Flensrocker.ToDo.ToDoCommandState")]
public class ToDoCommandState : IDomainEventVersionable, IToDoEventProjectable
{
  [Id(1)]
  public int Version { get; private set; }
  [Id(2)]
  public ToDoStatus Status { get; private set; }

  void IToDoEventProjectable.Apply(DomainEventMetadata metadata, ToDoEvent.ToDoMemorized payload)
  {
    Status = ToDoStatus.Open;
  }

  void IToDoEventProjectable.Apply(DomainEventMetadata metadata, ToDoEvent.ToDoCompleted payload)
  {
    Status = ToDoStatus.Completed;
  }

  void IToDoEventProjectable.Apply(DomainEventMetadata metadata, ToDoEvent.ToDoDismissed payload)
  {
    Status = ToDoStatus.Dismissed;
  }

  void IToDoEventProjectable.Apply(DomainEventMetadata metadata, ToDoEvent.ToDoReopened payload)
  {
    Status = ToDoStatus.Open;
  }

  public void Apply(DomainEvent @event)
  {
    Version = @event.Metadata.StreamVersion;

    ((IToDoEventProjectable)this).ApplyToDoEvent(@event);
  }
}
