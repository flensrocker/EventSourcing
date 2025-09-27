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
  private readonly IDomainEventStorage<ToDoEvent> _eventStore;

  private DomainEventObserver<ToDoEvent, ToDoDetailsView>? _observer;

  public ToDoDetailsViewGrain(IDomainEventStorage<ToDoEvent> eventStore)
  {
    _eventStore = eventStore;
  }

  private async Task<DomainEventObserver<ToDoEvent, ToDoDetailsView>> GetObserver()
  {
    _observer ??= await DomainEventObserver<ToDoEvent, ToDoDetailsView>.Create(_eventStore, this.GetPrimaryKey());
    return _observer;
  }

  public async Task OnSubscribed(IBroadcastChannelSubscription streamSubscription)
  {
    var observer = await GetObserver();
    await streamSubscription.Attach<DomainEvent<ToDoEvent>>(observer.OnNextAsync, observer.OnErrorAsync);
  }

  public async Task<ToDoDetailsView> GetView()
  {
    var observer = await GetObserver();
    return observer.View;
  }
}
