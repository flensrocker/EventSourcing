using System.Linq.Expressions;
using System.Text.Json.Serialization;
using Flensrocker.EventSourcing.Abstractions;

namespace Flensrocker.ToDo.Abstractions.Tasks;

public abstract partial record TaskView
{
  [GenerateSerializer]
  [Alias(TaskConstants.TaskView.ListItem)]
  public sealed record ListItem : TaskView, IDomainEventVersionable, ITaskEventProjectable, IWhereKey<ListItem>
  {
    public const int ProjectionVersion = 1;

    [Id(0)]
    [JsonInclude]
    public Guid TaskId { get; internal set; }

    [Id(1)]
    [JsonInclude]
    public int Version { get; internal set; }

    [Id(2)]
    [JsonInclude]
    public Guid? TenantId { get; internal set; }

    [Id(3)]
    [JsonInclude]
    public DateTimeOffset OpenedAt { get; internal set; }

    [Id(4)]
    [JsonInclude]
    public string Title { get; internal set; } = "";

    [Id(5)]
    [JsonInclude]
    public TaskStatus Status { get; internal set; }

    static object IWhereKey<ListItem>.GetKey(DomainEvent domainEvent) => domainEvent.Metadata.StreamId;

    static Expression<Func<ListItem, bool>> IWhereKey<ListItem>.GetWhereKey(DomainEvent domainEvent) =>
      item => item.TaskId == domainEvent.Metadata.StreamId;

    void ITaskEventProjectable.Apply(DomainEventMetadata metadata, TaskEvent.Completed payload)
    {
      Version = metadata.StreamVersion;
      Status = TaskStatus.Completed;
    }

    void ITaskEventProjectable.Apply(DomainEventMetadata metadata, TaskEvent.Dismissed payload)
    {
      Version = metadata.StreamVersion;
      Status = TaskStatus.Dismissed;
    }

    void ITaskEventProjectable.Apply(DomainEventMetadata metadata, TaskEvent.Opened payload)
    {
      TaskId = metadata.StreamId;
      Version = metadata.StreamVersion;
      TenantId = metadata.TenantId;
      OpenedAt = payload.OpenedAt;
      Title = payload.Title;
      Status = TaskStatus.Opened;
    }

    void ITaskEventProjectable.Apply(DomainEventMetadata metadata, TaskEvent.Paused payload)
    {
      Status = TaskStatus.Paused;
    }

    void ITaskEventProjectable.Apply(DomainEventMetadata metadata, TaskEvent.Reopened payload)
    {
      Version = metadata.StreamVersion;
      Status = TaskStatus.Opened;
    }

    void ITaskEventProjectable.Apply(DomainEventMetadata metadata, TaskEvent.Started payload)
    {
      Status = TaskStatus.Started;
    }

    public void Apply(DomainEvent @event)
    {
      Version = @event.Metadata.StreamVersion;

      ((ITaskEventProjectable)this).ApplyTaskEvent(@event);
    }
  }
}
