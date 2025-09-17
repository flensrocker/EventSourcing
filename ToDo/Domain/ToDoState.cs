using OrleansPlayground.Abstractions.Domain;

namespace OrleansPlayground.ToDo.Domain;

public enum ToDoStatus
{
  Open = 0,
  Completed = 1,
  Dismissed = 2,
}

public record ToDoState : IState<ToDoEvent>
{
  public string Title { get; private set; } = "";
  public ToDoStatus Status { get; private set; }
  public string? DismissReason { get; private set; }

  public ToDoState() { }

  public void Apply(ToDoEvent.ToDoMemorized @event)
  {
    Title = @event.Title;
    Status = ToDoStatus.Open;
    DismissReason = null;
  }

  public void Apply(ToDoEvent.ToDoCompleted @event)
  {
    Status = ToDoStatus.Completed;
    DismissReason = null;
  }

  public void Apply(ToDoEvent.ToDoDismissed @event)
  {
    Status = ToDoStatus.Dismissed;
    DismissReason = @event.Reason;
  }

  public void Apply(ToDoEvent.ToDoReopened @event)
  {
    Status = ToDoStatus.Open;
    DismissReason = null;
  }

  public void ApplyEvents(params IEnumerable<ToDoEvent> events)
  {
    foreach (var @event in events)
    {
      switch (@event)
      {
        case ToDoEvent.ToDoMemorized ev:
          Apply(ev);
          break;
        case ToDoEvent.ToDoCompleted ev:
          Apply(ev);
          break;
        case ToDoEvent.ToDoDismissed ev:
          Apply(ev);
          break;
        case ToDoEvent.ToDoReopened ev:
          Apply(ev);
          break;
      }
    }
  }
}
