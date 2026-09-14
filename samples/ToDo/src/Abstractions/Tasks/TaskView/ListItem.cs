using System.Linq.Expressions;
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
    public Guid TaskId { get; private set; }

    [Id(1)]
    public int Version { get; private set; }

    [Id(2)]
    public Guid? TenantId { get; private set; }

    [Id(3)]
    public DateTimeOffset CreatedAt { get; private set; }

    [Id(4)]
    public string Title { get; private set; } = "";

    [Id(5)]
    public TaskStatus Status { get; private set; }

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
