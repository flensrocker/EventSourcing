using Orleans.BroadcastChannel;
using OrleansPlayground.Abstractions.Domain;
using OrleansPlayground.Abstractions.Orleans;
using OrleansPlayground.Abstractions.Persistence;
using OrleansPlayground.ToDo.Domain;
using OrleansPlayground.ToDo.Orleans.Abstractions;
using OrleansPlayground.ToDo.Views;

namespace OrleansPlayground.ToDo.Orleans.Server;

[ImplicitChannelSubscription(ToDoNames.ToDoEventChannel)]
public class ToDoDetailsViewGrain : Grain, IOnBroadcastChannelSubscribed, IToDoDetailsViewGrain
{
  private readonly IDomainEventStore<ToDoEvent> _eventStore;

  private DomainEventObserver<ToDoEvent, ToDoDetailsView>? _observer;

  public ToDoDetailsViewGrain(IDomainEventStore<ToDoEvent> eventStore)
  {
    _eventStore = eventStore;
  }

  public async Task OnSubscribed(IBroadcastChannelSubscription streamSubscription)
  {
    _observer ??= await DomainEventObserver<ToDoEvent, ToDoDetailsView>.Create(_eventStore, this.GetPrimaryKey());

    await streamSubscription.Attach<DomainEvent<ToDoEvent>>(ev => _observer.OnNextAsync(ev, null), ex => _observer.OnErrorAsync(ex));
  }

  public Task<ToDoDetailsView?> GetView()
  {
    return Task.FromResult(_observer?.View);
  }
}
