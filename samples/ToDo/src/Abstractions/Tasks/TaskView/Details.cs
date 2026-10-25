using System.Text.Json.Serialization;
using Flensrocker.EventSourcing.Abstractions;

namespace Flensrocker.ToDo.Abstractions.Tasks;

public abstract partial record TaskView
{
  [GenerateSerializer]
  [Alias(TaskConstants.TaskView.Details)]
  public sealed record Details : TaskView, IDomainEventVersionable, ITaskEventProjectable
  {
    [Id(0)]
    [JsonInclude]
    public Guid TaskId { get; internal set; }

    [Id(1)]
    [JsonInclude]
    public int Version { get; internal set; }

    [Id(2)]
    [JsonInclude]
    public string Title { get; internal set; } = "";

    [Id(3)]
    [JsonInclude]
    public TaskStatus Status { get; internal set; }

    [Id(4)]
    [JsonInclude]
    public string? DismissReason { get; internal set; }

    [Id(5)]
    [JsonInclude]
    public Guid? StartedByUserId { get; internal set; }

    void ITaskEventProjectable.Apply(DomainEventMetadata metadata, TaskEvent.Completed payload)
    {
      Status = TaskStatus.Completed;
      DismissReason = null;
      StartedByUserId = null;
    }

    void ITaskEventProjectable.Apply(DomainEventMetadata metadata, TaskEvent.Dismissed payload)
    {
      Status = TaskStatus.Dismissed;
      DismissReason = payload.Reason;
      StartedByUserId = null;
    }

    void ITaskEventProjectable.Apply(DomainEventMetadata metadata, TaskEvent.Noted payload)
    {
      Title = payload.Title;
      Status = TaskStatus.Opened;
      DismissReason = null;
      StartedByUserId = null;
    }

    void ITaskEventProjectable.Apply(DomainEventMetadata metadata, TaskEvent.Paused payload)
    {
      Status = TaskStatus.Paused;
      StartedByUserId = null;
    }

    void ITaskEventProjectable.Apply(DomainEventMetadata metadata, TaskEvent.Reopened payload)
    {
      Status = TaskStatus.Opened;
      DismissReason = null;
      StartedByUserId = null;
    }

    void ITaskEventProjectable.Apply(DomainEventMetadata metadata, TaskEvent.Started payload)
    {
      Status = TaskStatus.Started;
      StartedByUserId = payload.ByUserId;
    }

    public void Apply(DomainEvent @event)
    {
      TaskId = @event.Metadata.StreamId;
      Version = @event.Metadata.StreamVersion;

      ((ITaskEventProjectable)this).ApplyTaskEvent(@event);
    }
  }
}
