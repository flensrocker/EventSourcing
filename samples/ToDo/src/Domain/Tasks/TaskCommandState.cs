using Flensrocker.EventSourcing.Abstractions;
using Flensrocker.ToDo.Abstractions.Tasks;

namespace Flensrocker.ToDo.Domain.Tasks;

[GenerateSerializer]
[Alias("ToDo_TaskCommandState")]
public sealed class TaskCommandState : IDomainEventVersionable, ITaskEventProjectable
{
  [Id(0)]
  public int Version { get; private set; }

  [Id(1)]
  public Abstractions.Tasks.TaskStatus Status { get; private set; }

  [Id(2)]
  public string? DismissReason { get; private set; }

  void ITaskEventProjectable.Apply(DomainEventMetadata metadata, TaskEvent.Completed payload)
  {
    Status = Abstractions.Tasks.TaskStatus.Completed;
    DismissReason = null;
  }

  void ITaskEventProjectable.Apply(DomainEventMetadata metadata, TaskEvent.Dismissed payload)
  {
    Status = Abstractions.Tasks.TaskStatus.Dismissed;
    DismissReason = payload.Reason;
  }

  void ITaskEventProjectable.Apply(DomainEventMetadata metadata, TaskEvent.Noted payload)
  {
    Status = Abstractions.Tasks.TaskStatus.Open;
    DismissReason = null;
  }

  void ITaskEventProjectable.Apply(DomainEventMetadata metadata, TaskEvent.Reopened payload)
  {
    Status = Abstractions.Tasks.TaskStatus.Open;
    DismissReason = null;
  }

  public void Apply(DomainEvent @event)
  {
    Version = @event.Metadata.StreamVersion;

    ((ITaskEventProjectable)this).ApplyTaskEvent(@event);
  }
}
