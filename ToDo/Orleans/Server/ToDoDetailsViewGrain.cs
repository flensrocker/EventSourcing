using Orleans.Streams;
using OrleansPlayground.Abstractions.Domain;
using OrleansPlayground.Abstractions.EntityFrameworkCore;
using OrleansPlayground.ToDo.Domain;
using OrleansPlayground.ToDo.Orleans.Abstractions;
using OrleansPlayground.ToDo.Views;

namespace OrleansPlayground.ToDo.Orleans.Server;

public class ToDoDetailsViewGrain : Grain, IToDoDetailsViewGrain, IAsyncObserver<DomainEvent<ToDoEvent>>
{
  private readonly IDomainEventStore<ToDoEvent> _eventStore;

  private int _viewVersion;
  private ToDoDetailsView? _view;
  private StreamSubscriptionHandle<DomainEvent<ToDoEvent>>? _subscriptionHandle;

  public ToDoDetailsViewGrain(IDomainEventStore<ToDoEvent> eventStore)
  {
    _eventStore = eventStore;
  }

  public override async Task OnActivateAsync(CancellationToken cancellationToken)
  {
    var (version, view) = await _eventStore.ReadStateFromStorage<ToDoDetailsView>(this.GetPrimaryKey());

    _viewVersion = version;
    if (version > 0)
      _view = view;

    var streamProvider = this.GetStreamProvider("StreamProvider");
    var streamId = StreamId.Create("ToDoEvent", this.GetPrimaryKey());
    var stream = streamProvider.GetStream<DomainEvent<ToDoEvent>>(streamId);
    _subscriptionHandle = await stream.SubscribeAsync(this);

    await base.OnActivateAsync(cancellationToken);
  }

  public override async Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
  {
    if (_subscriptionHandle is not null)
    {
      await _subscriptionHandle.UnsubscribeAsync();
      _subscriptionHandle = null;
    }

    await base.OnDeactivateAsync(reason, cancellationToken);
  }

  public Task<ToDoDetailsView?> GetView()
  {
    return Task.FromResult(_view);
  }

  public async Task OnNextAsync(DomainEvent<ToDoEvent> item, StreamSequenceToken? token = null)
  {
    if (item.Metadata.EntityVersion <= _viewVersion)
      return;

    _view ??= new();

    if (item.Metadata.EntityVersion > _viewVersion + 1)
    {
      var missingEvents = await _eventStore.ReadEventsFromStorage(this.GetPrimaryKey(), _viewVersion + 1, item.Metadata.EntityVersion);
      if (missingEvents is { Count: > 0 })
        _view.ApplyEvents(missingEvents);
    }

    _view.Apply(item);
    // TODO push updated view somewhere... stream? IAsyncEnumerable?

    return;
  }

  public Task OnErrorAsync(Exception ex)
  {
    // TODO Error-Handling
    return Task.CompletedTask;
  }
}
