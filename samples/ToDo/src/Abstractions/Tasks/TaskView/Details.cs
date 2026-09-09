using Flensrocker.EventSourcing.Abstractions;

namespace Flensrocker.ToDo.Abstractions.Tasks;

public abstract partial record TaskView
{
  [GenerateSerializer]
  [Alias(TaskConstants.TaskView.Details)]
  public sealed record Details : TaskView, IDomainEventVersionable, ITaskEventProjectable
  {
    [Id(0)]
    public int Version { get; private set; }

    [Id(1)]
    public string Title { get; private set; } = "";

    [Id(2)]
    public TaskStatus Status { get; private set; }

    [Id(3)]
    public string? DismissReason { get; private set; }

    void ITaskEventProjectable.Apply(DomainEventMetadata metadata, TaskEvent.Completed payload)
    {
      Status = TaskStatus.Completed;
      DismissReason = null;
    }

    void ITaskEventProjectable.Apply(DomainEventMetadata metadata, TaskEvent.Dismissed payload)
    {
      Status = TaskStatus.Dismissed;
      DismissReason = payload.Reason;
    }

    void ITaskEventProjectable.Apply(DomainEventMetadata metadata, TaskEvent.Memorized payload)
    {
      Title = payload.Title;
      Status = TaskStatus.Open;
      DismissReason = null;
    }

    void ITaskEventProjectable.Apply(DomainEventMetadata metadata, TaskEvent.Reopened payload)
    {
      Status = TaskStatus.Open;
      DismissReason = null;
    }

    public void Apply(DomainEvent @event)
    {
      Version = @event.Metadata.StreamVersion;

      ((ITaskEventProjectable)this).ApplyTaskEvent(@event);
    }
  }
}
