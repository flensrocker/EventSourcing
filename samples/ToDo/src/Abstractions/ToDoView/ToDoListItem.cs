using System.Linq.Expressions;
using Flensrocker.Domain.Abstractions;

namespace Flensrocker.ToDo.Abstractions;

public abstract partial record ToDoView
{
  public sealed record ToDoListItem : ToDoView, IDomainEventVersionable, IToDoEventProjectable, IWhereId<ToDoListItem>
  {
    public Guid ToDoId { get; private set; }
    public int Version { get; private set; }
    public Guid? TenantId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public string Title { get; private set; } = "";
    public ToDoStatus Status { get; private set; }

    static Expression<Func<ToDoListItem, bool>> IWhereId<ToDoListItem>.GetWhereId(Guid id)
      => item => item.ToDoId == id;

    void IToDoEventProjectable.Apply(DomainEventMetadata metadata, ToDoEvent.ToDoMemorized payload)
    {
      ToDoId = metadata.StreamId;
      Version = metadata.StreamVersion;
      TenantId = metadata.TenantId;
      CreatedAt = payload.CreatedAt;
      Title = payload.Title;
      Status = ToDoStatus.Open;
    }

    void IToDoEventProjectable.Apply(DomainEventMetadata metadata, ToDoEvent.ToDoCompleted payload)
    {
      Version = metadata.StreamVersion;
      Status = ToDoStatus.Completed;
    }

    void IToDoEventProjectable.Apply(DomainEventMetadata metadata, ToDoEvent.ToDoDismissed payload)
    {
      Version = metadata.StreamVersion;
      Status = ToDoStatus.Dismissed;
    }

    void IToDoEventProjectable.Apply(DomainEventMetadata metadata, ToDoEvent.ToDoReopened payload)
    {
      Version = metadata.StreamVersion;
      Status = ToDoStatus.Open;
    }

    public void Apply(DomainEvent @event)
    {
      Version = @event.Metadata.StreamVersion;

      ((IToDoEventProjectable)this).ApplyToDoEvent(@event);
    }
  }
}
