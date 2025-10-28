using Flensrocker.Domain.Abstractions;
using Flensrocker.ToDo.Abstractions;

namespace Flensrocker.ToDo.Domain;

public partial class ToDoAggregate : IDomainAggregate<ToDoCommand, ToDoEvent>, IDomainEventVersionable, IToDoEventProjectable
{
  public int Version { get; private set; }
  public ToDoStatus Status { get; private set; }

  void IToDoEventProjectable.Apply(DomainEventMetadata metadata, ToDoEvent.ToDoMemorized payload)
  {
    Status = ToDoStatus.Open;
  }

  void IToDoEventProjectable.Apply(DomainEventMetadata metadata, ToDoEvent.ToDoCompleted payload)
  {
    Status = ToDoStatus.Completed;
  }

  void IToDoEventProjectable.Apply(DomainEventMetadata metadata, ToDoEvent.ToDoDismissed payload)
  {
    Status = ToDoStatus.Dismissed;
  }

  void IToDoEventProjectable.Apply(DomainEventMetadata metadata, ToDoEvent.ToDoReopened payload)
  {
    Status = ToDoStatus.Open;
  }

  public void Apply(DomainEvent<ToDoEvent> @event)
  {
    Version = @event.Metadata.StreamVersion;

    ((IToDoEventProjectable)this).ApplyToDoEvent(@event);
  }

  public ToDoEvent[]? ExecuteCommand(ToDoCommand command)
  {
    return command switch
    {
      ToDoCommand.MemorizeToDo memorizeToDo => Memorize(memorizeToDo),
      ToDoCommand.CompleteToDo completeToDo => Complete(completeToDo),
      ToDoCommand.DismissToDo dismissToDo => Dismiss(dismissToDo),
      ToDoCommand.ReopenToDo reopenToDo => Reopen(reopenToDo),
      _ => throw new NotImplementedException($"command type {command.GetType().GetTypeName()}"),
    };
  }
}
