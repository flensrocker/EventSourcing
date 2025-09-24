using Orleans.Streams;
using OrleansPlayground.Abstractions.Domain;
using OrleansPlayground.Abstractions.EntityFrameworkCore;
using OrleansPlayground.Abstractions.Orleans;
using OrleansPlayground.ToDo.Domain;
using OrleansPlayground.ToDo.Orleans.Abstractions;
using OrleansPlayground.ToDo.Views;

namespace OrleansPlayground.ToDo.Orleans.Server;

public class ToDoDetailsViewGrain : Grain, IToDoDetailsViewGrain
{
  private readonly IDomainEventStore<ToDoEvent> _eventStore;

  private DomainEventObserver<ToDoEvent, ToDoDetailsView>? _observer;
  private StreamSubscriptionHandle<DomainEvent<ToDoEvent>>? _subscriptionHandle;

  public ToDoDetailsViewGrain(IDomainEventStore<ToDoEvent> eventStore)
  {
    _eventStore = eventStore;
  }

  public override async Task OnActivateAsync(CancellationToken cancellationToken)
  {
    _observer = await DomainEventObserver<ToDoEvent, ToDoDetailsView>.Create(_eventStore, this.GetPrimaryKey());

    var streamProvider = this.GetStreamProvider("StreamProvider");
    var streamId = StreamId.Create("ToDoEvent", this.GetPrimaryKey());
    var stream = streamProvider.GetStream<DomainEvent<ToDoEvent>>(streamId);
    _subscriptionHandle = await stream.SubscribeAsync(_observer);

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
    return Task.FromResult(_observer?.View);
  }
}
