using Flensrocker.EventSourcing.Abstractions;
using Flensrocker.ToDo.Abstractions.Tasks;

namespace Flensrocker.ToDo.Domain.Tasks;

[GenerateSerializer]
[Alias("ToDo_TaskCommandState")]
public class TaskCommandState : IDomainEventVersionable, ITaskEventProjectable
{
  [Id(0)]
  public int Version { get; private set; }

  [Id(1)]
  public Abstractions.Tasks.TaskStatus Status { get; private set; }

  void ITaskEventProjectable.Apply(DomainEventMetadata metadata, TaskEvent.Completed payload)
  {
    Status = Abstractions.Tasks.TaskStatus.Completed;
  }

  void ITaskEventProjectable.Apply(DomainEventMetadata metadata, TaskEvent.Dismissed payload)
  {
    Status = Abstractions.Tasks.TaskStatus.Dismissed;
  }

  void ITaskEventProjectable.Apply(DomainEventMetadata metadata, TaskEvent.Memorized payload)
  {
    Status = Abstractions.Tasks.TaskStatus.Open;
  }

  void ITaskEventProjectable.Apply(DomainEventMetadata metadata, TaskEvent.Reopened payload)
  {
    Status = Abstractions.Tasks.TaskStatus.Open;
  }

  public void Apply(DomainEvent @event)
  {
    Version = @event.Metadata.StreamVersion;

    ((ITaskEventProjectable)this).ApplyTaskEvent(@event);
  }
}
