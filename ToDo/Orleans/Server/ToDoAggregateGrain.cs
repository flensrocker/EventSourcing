using Orleans.EventSourcing.CustomStorage;
using Orleans.Providers;
using Orleans.Streams;
using OrleansPlayground.Abstractions.Domain;
using OrleansPlayground.Abstractions.EntityFrameworkCore;
using OrleansPlayground.Abstractions.Orleans;
using OrleansPlayground.ToDo.Domain;
using OrleansPlayground.ToDo.Orleans.Abstractions;

namespace OrleansPlayground.ToDo.Orleans.Server;

[LogConsistencyProvider(ProviderName = "ToDoStorage")]
public class ToDoAggregateGrain : DomainEventSourcedGrain<ToDoAggregate, ToDoEvent>, IToDoAggregateGrain
{
  private IAsyncStream<DomainEvent<ToDoEvent>>? _stream;

  public ToDoAggregateGrain(IDomainEventStore<ToDoEvent> eventStore)
    : base(eventStore)
  {
  }

  public override async Task OnActivateAsync(CancellationToken cancellationToken)
  {
    await base.OnActivateAsync(cancellationToken);

    var streamProvider = this.GetStreamProvider("StreamProvider");
    var streamId = StreamId.Create("ToDoEvent", this.GetPrimaryKey());
    _stream = streamProvider.GetStream<DomainEvent<ToDoEvent>>(streamId);
  }

  private async Task<bool> Execute<TCommand>(TCommand? command, Func<TCommand, ToDoEvent?> handler)
  {
    if (command is null)
      return false;

    var @event = handler(command);
    if (@event is null)
      return false;

    var domainEvent = RaiseDomainEvent(@event);
    await ConfirmEvents();

    if (_stream is not null)
      await _stream.OnNextAsync(domainEvent);

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
