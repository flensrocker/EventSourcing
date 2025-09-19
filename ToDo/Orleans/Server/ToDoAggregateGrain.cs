using Orleans.EventSourcing.CustomStorage;
using Orleans.Providers;
using OrleansPlayground.Abstractions.EntityFrameworkCore;
using OrleansPlayground.Abstractions.Orleans;
using OrleansPlayground.ToDo.Domain;
using OrleansPlayground.ToDo.Orleans.Abstractions;

namespace OrleansPlayground.ToDo.Orleans.Server;

[GenerateSerializer]
public record MemorizeToDoDto
{
  [Id(0)]
  public string? Title { get; init; }
}

[GenerateSerializer]
public record DismissToDoDto
{
  [Id(0)]
  public string? Reason { get; init; }
}

[LogConsistencyProvider(ProviderName = "ToDoStorage")]
public class ToDoAggregateGrain : DomainEventSourcedGrain<ToDoAggregate, ToDoEvent>, IToDoAggregateGrain
{
  public ToDoAggregateGrain(IDomainEventStore<Guid, ToDoEvent> eventStore)
    : base(eventStore)
  {
  }

  private async Task<bool> Execute<TCommand, TEvent>(TCommand? command, Func<TCommand, TEvent?> handler)
    where TEvent: ToDoEvent
  {
    if (command is null)
      return false;

    var @event = handler(command);
    if (@event is null)
      return false;

    RaiseDomainEvent(@event);
    await ConfirmEvents();

    // TODO Publish ToDo-Events

    return true;
  }

  public async Task<bool> Memorize(MemorizeToDoDto dto)
  {
    var command = ToDoCommand.MemorizeToDo.Create(dto.Title);
    return await Execute(command, State.Memorize);
  }

  public async Task<bool> Complete()
  {
    var command = ToDoCommand.CompleteToDo.Create();
    return await Execute(command, State.Complete);
  }

  public async Task<bool> Dismiss(DismissToDoDto dto)
  {
    var command = ToDoCommand.DismissToDo.Create(dto.Reason);
    return await Execute(command, State.Dismiss);
  }

  public async Task<bool> Reopen()
  {
    var command = ToDoCommand.ReopenToDo.Create();
    return await Execute(command, State.Reopen);
  }
}
