using Orleans.BroadcastChannel;
using Orleans.EventSourcing.CustomStorage;
using Orleans.Providers;
using OrleansPlayground.Abstractions.Domain;
using OrleansPlayground.Abstractions.Orleans;
using OrleansPlayground.Abstractions.Persistence;
using OrleansPlayground.ToDo.Domain;
using OrleansPlayground.ToDo.Orleans.Abstractions;

namespace OrleansPlayground.ToDo.Orleans.Server;

[LogConsistencyProvider(ProviderName = ToDoNames.ToDoEventStorageProvider)]
public class ToDoAggregateGrain : DomainAggregateGrain<ToDoAggregate, ToDoEvent>, IToDoAggregateGrain
{
  private readonly IClusterClient _clusterClient;
  private IBroadcastChannelWriter<DomainEvent<ToDoEvent>>? _channel;

  public ToDoAggregateGrain(IClusterClient clusterClient, IDomainEventStore<ToDoEvent> eventStore)
    : base(eventStore)
  {
    _clusterClient = clusterClient;
  }

  public override async Task OnActivateAsync(CancellationToken cancellationToken)
  {
    await base.OnActivateAsync(cancellationToken);

    var channelProvider = _clusterClient.GetBroadcastChannelProvider(ToDoNames.ToDoEventChannelProvider);
    var channelId = ChannelId.Create(ToDoNames.ToDoEventChannel, this.GetPrimaryKey());
    _channel = channelProvider.GetChannelWriter<DomainEvent<ToDoEvent>>(channelId);
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

    if (_channel is not null)
      await _channel.Publish(domainEvent);

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
