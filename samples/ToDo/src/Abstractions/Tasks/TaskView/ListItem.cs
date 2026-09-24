using System.Linq.Expressions;
using System.Text.Json.Serialization;
using Flensrocker.EventSourcing.Abstractions;

namespace Flensrocker.ToDo.Abstractions.Tasks;

public abstract partial record TaskView
{
  [GenerateSerializer]
  [Alias(TaskConstants.TaskView.ListItem)]
  public sealed record ListItem : TaskView, IDomainEventVersionable, ITaskEventProjectable, IWhereId<ListItem>
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
    public DateTimeOffset CreatedAt { get; internal set; }

    [Id(4)]
    [JsonInclude]
    public string Title { get; internal set; } = "";

    [Id(5)]
    [JsonInclude]
    public TaskStatus Status { get; internal set; }

    static Expression<Func<ListItem, bool>> IWhereId<ListItem>.GetWhereId(Guid id) => item => item.TaskId == id;

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

    void ITaskEventProjectable.Apply(DomainEventMetadata metadata, TaskEvent.Noted payload)
    {
      TaskId = metadata.StreamId;
      Version = metadata.StreamVersion;
      TenantId = metadata.TenantId;
      CreatedAt = payload.CreatedAt;
      Title = payload.Title;
      Status = TaskStatus.Open;
    }

    void ITaskEventProjectable.Apply(DomainEventMetadata metadata, TaskEvent.Reopened payload)
    {
      Version = metadata.StreamVersion;
      Status = TaskStatus.Open;
    }

    public void Apply(DomainEvent @event)
    {
      Version = @event.Metadata.StreamVersion;

      ((ITaskEventProjectable)this).ApplyTaskEvent(@event);
    }
  }
}
