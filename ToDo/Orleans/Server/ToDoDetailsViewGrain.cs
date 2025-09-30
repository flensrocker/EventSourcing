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

  private DomainEventProjection<ToDoEvent, ToDoDetailsView>? _viewProjection;
  private StreamSubscriptionHandle<DomainEvent<ToDoEvent>>? _viewSubscription;

  public ToDoDetailsViewGrain(IDomainEventStorage<ToDoEvent> eventStore)
  {
    _eventStore = eventStore;
  }

  private async Task<DomainEventProjection<ToDoEvent, ToDoDetailsView>> GetViewProjection()
  {
    _viewProjection ??= await DomainEventProjection<ToDoEvent, ToDoDetailsView>.Create(_eventStore, this.GetPrimaryKey());
    return _viewProjection;
  }

  public override async Task OnActivateAsync(CancellationToken cancellationToken)
  {
    var streamProvider = this.GetStreamProvider(ToDoNames.ToDoEventStreamProvider);
    var streamId = StreamId.Create(ToDoNames.ToDoEventStream, this.GetPrimaryKey());
    var stream = streamProvider.GetStream<DomainEvent<ToDoEvent>>(streamId);

    var viewProjection = await GetViewProjection();
    _viewSubscription = await stream.SubscribeAsync(viewProjection);

    await base.OnActivateAsync(cancellationToken);
  }

  public override async Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
  {
    _viewProjection = null;

    if (_viewSubscription is not null)
    {
      await _viewSubscription.UnsubscribeAsync();
      _viewSubscription = null;
    }

    await base.OnDeactivateAsync(reason, cancellationToken);
  }

  public async Task<ToDoDetailsView?> GetView()
  {
    var viewProjection = await GetViewProjection();

    if (viewProjection.View is { Version: > 0 })
      return viewProjection.View;

    DeactivateOnIdle();
    return null;
  }
}
