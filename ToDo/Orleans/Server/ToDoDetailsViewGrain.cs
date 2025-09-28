using Orleans.Streams;
using OrleansPlayground.Abstractions.Domain;
using OrleansPlayground.Abstractions.Orleans;
using OrleansPlayground.Abstractions.Persistence;
using OrleansPlayground.ToDo.Domain;
using OrleansPlayground.ToDo.Orleans.Abstractions;
using OrleansPlayground.ToDo.Views;

namespace OrleansPlayground.ToDo.Orleans.Server;

public class ToDoDetailsViewGrain : Grain, IToDoDetailsViewGrain
{
  private readonly IDomainEventStorage<ToDoEvent> _eventStore;

  private DomainEventProjection<ToDoEvent, ToDoDetailsView>? _observer;
  private StreamSubscriptionHandle<DomainEvent<ToDoEvent>>? _subscriptionHandle;

  public ToDoDetailsViewGrain(IDomainEventStorage<ToDoEvent> eventStore)
  {
    _eventStore = eventStore;
  }

  private async Task<DomainEventProjection<ToDoEvent, ToDoDetailsView>> GetObserver()
  {
    _observer ??= await DomainEventProjection<ToDoEvent, ToDoDetailsView>.Create(_eventStore, this.GetPrimaryKey());
    return _observer;
  }

  public override async Task OnActivateAsync(CancellationToken cancellationToken)
  {
    var streamProvider = this.GetStreamProvider(ToDoNames.ToDoEventStreamProvider);
    var streamId = StreamId.Create(ToDoNames.ToDoEventStream, this.GetPrimaryKey());
    var stream = streamProvider.GetStream<DomainEvent<ToDoEvent>>(streamId);

    var observer = await GetObserver();
    _subscriptionHandle = await stream.SubscribeAsync(observer);

    await base.OnActivateAsync(cancellationToken);
  }

  public override async Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
  {
    _observer = null;

    if (_subscriptionHandle is not null)
    {
      await _subscriptionHandle.UnsubscribeAsync();
      _subscriptionHandle = null;
    }

    await base.OnDeactivateAsync(reason, cancellationToken);
  }

  public async Task<ToDoDetailsView> GetView()
  {
    var observer = await GetObserver();
    return observer.View;
  }
}
