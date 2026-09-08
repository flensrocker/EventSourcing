using System.Linq.Expressions;
using Flensrocker.EventSourcing.Abstractions;

namespace Flensrocker.ToDo.Abstractions;

public abstract partial record ToDoView
{
  [GenerateSerializer]
  [Alias(ToDoConstants.ToDoView.ListItem)]
  public sealed record ListItem : ToDoView, IDomainEventVersionable, IToDoEventProjectable, IWhereId<ListItem>
  {
    public const int ProjectionVersion = 1;

    [Id(0)]
    public Guid ToDoId { get; private set; }

    [Id(1)]
    public int Version { get; private set; }

    [Id(2)]
    public Guid? TenantId { get; private set; }

    [Id(3)]
    public DateTimeOffset CreatedAt { get; private set; }

    [Id(4)]
    public string Title { get; private set; } = "";

    [Id(5)]
    public ToDoStatus Status { get; private set; }

    static Expression<Func<ListItem, bool>> IWhereId<ListItem>.GetWhereId(Guid id) => item => item.ToDoId == id;

    void IToDoEventProjectable.Apply(DomainEventMetadata metadata, ToDoEvent.Completed payload)
    {
      Version = metadata.StreamVersion;
      Status = ToDoStatus.Completed;
    }

    void IToDoEventProjectable.Apply(DomainEventMetadata metadata, ToDoEvent.Dismissed payload)
    {
      Version = metadata.StreamVersion;
      Status = ToDoStatus.Dismissed;
    }

    void IToDoEventProjectable.Apply(DomainEventMetadata metadata, ToDoEvent.Memorized payload)
    {
      ToDoId = metadata.StreamId;
      Version = metadata.StreamVersion;
      TenantId = metadata.TenantId;
      CreatedAt = payload.CreatedAt;
      Title = payload.Title;
      Status = ToDoStatus.Open;
    }

    void IToDoEventProjectable.Apply(DomainEventMetadata metadata, ToDoEvent.Reopened payload)
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
