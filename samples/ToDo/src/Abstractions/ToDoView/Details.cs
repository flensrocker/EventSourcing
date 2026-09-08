using Flensrocker.EventSourcing.Abstractions;

namespace Flensrocker.ToDo.Abstractions;

public abstract partial record ToDoView
{
  [GenerateSerializer]
  [Alias(ToDoConstants.ToDoView.Details)]
  public sealed record Details : ToDoView, IDomainEventVersionable, IToDoEventProjectable
  {
    [Id(0)]
    public int Version { get; private set; }

    [Id(1)]
    public string Title { get; private set; } = "";

    [Id(2)]
    public ToDoStatus Status { get; private set; }

    [Id(3)]
    public string? DismissReason { get; private set; }

    void IToDoEventProjectable.Apply(DomainEventMetadata metadata, ToDoEvent.Completed payload)
    {
      Status = ToDoStatus.Completed;
      DismissReason = null;
    }

    void IToDoEventProjectable.Apply(DomainEventMetadata metadata, ToDoEvent.Dismissed payload)
    {
      Status = ToDoStatus.Dismissed;
      DismissReason = payload.Reason;
    }

    void IToDoEventProjectable.Apply(DomainEventMetadata metadata, ToDoEvent.Memorized payload)
    {
      Title = payload.Title;
      Status = ToDoStatus.Open;
      DismissReason = null;
    }

    void IToDoEventProjectable.Apply(DomainEventMetadata metadata, ToDoEvent.Reopened payload)
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
