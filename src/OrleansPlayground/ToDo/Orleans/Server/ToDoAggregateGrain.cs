using Flensrocker.Domain.Abstractions;
using Flensrocker.Orleans.Domain.Abstractions;
using Orleans;
using Orleans.EventSourcing.CustomStorage;
using Orleans.Providers;
using Orleans.Runtime;
using Orleans.Streams;
using OrleansPlayground.ToDo.Abstractions;
using OrleansPlayground.ToDo.Domain;
using OrleansPlayground.ToDo.Orleans.Abstractions;

namespace OrleansPlayground.ToDo.Orleans.Server;

[LogConsistencyProvider(ProviderName = ToDoNames.ToDoEventStorageProvider)]
public class ToDoAggregateGrain : DomainAggregateGrain<ToDoAggregate, ToDoEvent>, IToDoAggregateGrain
{
  private IAsyncStream<DomainEvent<ToDoEvent>>? _stream;

  public override async Task OnActivateAsync(CancellationToken cancellationToken)
  {
    await base.OnActivateAsync(cancellationToken);

    var streamProvider = this.GetStreamProvider(ToDoNames.ToDoEventStreamProvider);
    var streamId = StreamId.Create(ToDoNames.ToDoEventStream, this.GetPrimaryKey());
    _stream = streamProvider.GetStream<DomainEvent<ToDoEvent>>(streamId);
  }

  public async Task<bool> ExecuteCommand(ToDoCommand command)
  {
    var events = State.ExecuteCommand(command);
    if (events is not { Length: > 0 })
      return false;

    var domainEvents = RaiseDomainEvents(events);
    await ConfirmEvents();

    // TODO Transaction-Gap between stored and published events

    if (_stream is not null)
      await _stream.OnNextBatchAsync(domainEvents);

    return true;
  }
}
