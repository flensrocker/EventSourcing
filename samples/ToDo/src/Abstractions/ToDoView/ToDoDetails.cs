using Flensrocker.EventSourcing.Abstractions;

namespace Flensrocker.ToDo.Abstractions;

public abstract partial record ToDoView
{
  [GenerateSerializer]
  [Alias("ToDo.ToDoView.ToDoDetails")]
  public sealed record ToDoDetails : ToDoView, IDomainEventVersionable, IToDoEventProjectable
  {
    [Id(0)]
    public int Version { get; private set; }
    [Id(1)]
    public string Title { get; private set; } = "";
    [Id(2)]
    public ToDoStatus Status { get; private set; }
    [Id(3)]
    public string? DismissReason { get; private set; }

    void IToDoEventProjectable.Apply(DomainEventMetadata metadata, ToDoEvent.ToDoMemorized payload)
    {
      Title = payload.Title;
      Status = ToDoStatus.Open;
      DismissReason = null;
    }

    void IToDoEventProjectable.Apply(DomainEventMetadata metadata, ToDoEvent.ToDoCompleted payload)
    {
      Status = ToDoStatus.Completed;
      DismissReason = null;
    }

    void IToDoEventProjectable.Apply(DomainEventMetadata metadata, ToDoEvent.ToDoDismissed payload)
    {
      Status = ToDoStatus.Dismissed;
      DismissReason = payload.Reason;
    }

    void IToDoEventProjectable.Apply(DomainEventMetadata metadata, ToDoEvent.ToDoReopened payload)
    {
      Status = ToDoStatus.Open;
      DismissReason = null;
    }

    public void Apply(DomainEvent @event)
    {
      Version = @event.Metadata.StreamVersion;

      ((IToDoEventProjectable)this).ApplyToDoEvent(@event);
    }
  }
}
