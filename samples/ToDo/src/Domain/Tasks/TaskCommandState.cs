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

  [Id(3)]
  public Guid? StartedByUserId { get; private set; }

  void ITaskEventProjectable.Apply(DomainEventMetadata metadata, TaskEvent.Completed payload)
  {
    Status = Abstractions.Tasks.TaskStatus.Completed;
    DismissReason = null;
    StartedByUserId = null;
  }

  void ITaskEventProjectable.Apply(DomainEventMetadata metadata, TaskEvent.Dismissed payload)
  {
    Status = Abstractions.Tasks.TaskStatus.Dismissed;
    DismissReason = payload.Reason;
    StartedByUserId = null;
  }

  void ITaskEventProjectable.Apply(DomainEventMetadata metadata, TaskEvent.Noted payload)
  {
    Status = Abstractions.Tasks.TaskStatus.Open;
    DismissReason = null;
    StartedByUserId = null;
  }

  void ITaskEventProjectable.Apply(DomainEventMetadata metadata, TaskEvent.Paused payload)
  {
    Status = Abstractions.Tasks.TaskStatus.Paused;
    StartedByUserId = null;
  }

  void ITaskEventProjectable.Apply(DomainEventMetadata metadata, TaskEvent.Reopened payload)
  {
    Status = Abstractions.Tasks.TaskStatus.Open;
    DismissReason = null;
    StartedByUserId = null;
  }

  void ITaskEventProjectable.Apply(DomainEventMetadata metadata, TaskEvent.Started payload)
  {
    Status = Abstractions.Tasks.TaskStatus.Started;
    StartedByUserId = payload.ByUserId;
  }

  public void Apply(DomainEvent @event)
  {
    Version = @event.Metadata.StreamVersion;

    ((ITaskEventProjectable)this).ApplyTaskEvent(@event);
  }
}
