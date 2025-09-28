using Orleans.BroadcastChannel;
using Orleans.Streams;
using Orleans.Streams.Core;
using OrleansPlayground.Abstractions.Domain;
using OrleansPlayground.ToDo.Domain;
using OrleansPlayground.ToDo.Orleans.Abstractions;
using OrleansPlayground.ToDo.Views;

namespace OrleansPlayground.ToDo.Orleans.Server;

[ImplicitChannelSubscription(ToDoNames.ToDoEventChannel)]
public class ToDoEventRouterGrain : Grain, IGrainWithGuidKey, IOnBroadcastChannelSubscribed
{
  public async Task OnSubscribed(IBroadcastChannelSubscription streamSubscription)
  {
    await streamSubscription.Attach<DomainEvent<ToDoEvent>>(OnNextAsync, OnErrorAsync);
  }

  private async Task OnNextAsync(DomainEvent<ToDoEvent> @event)
  {
    var tenantId = Guid.Empty; // TODO derive from @event
    var tenantStreamId = StreamId.Create("ToDoListView", tenantId);
    var tenantStream = this.GetStreamProvider("StreamProvider").GetStream<DomainEvent<ToDoEvent>>(tenantStreamId);

    await tenantStream.OnNextAsync(@event);
  }

  private Task OnErrorAsync(Exception ex)
  {
    // TODO add error handling
    return Task.CompletedTask;
  }
}

[ImplicitStreamSubscription("ToDoListView")]
public class ToDoListViewGrain : Grain, IStreamSubscriptionObserver, IAsyncObserver<DomainEvent<ToDoEvent>>, IToDoListViewGrain
{
  private readonly ToDoListView _view;

  public ToDoListViewGrain()
  {
    // TODO load from persistence etc.
    _view = new ToDoListView
    {
      TenantId = this.GetPrimaryKey(),
    };
  }

  public async Task OnSubscribed(IStreamSubscriptionHandleFactory handleFactory)
  {
    var handle = handleFactory.Create<DomainEvent<ToDoEvent>>();
    await handle.ResumeAsync(this);
  }

  public Task OnNextAsync(DomainEvent<ToDoEvent> item, StreamSequenceToken? token = null)
  {
    _view.Apply(item);
    return Task.CompletedTask;
  }

  public Task OnErrorAsync(Exception ex)
  {
    // TODO add error handling
    return Task.CompletedTask;
  }

  public Task<ToDoListItemView[]> GetList()
  {
    var items = _view.ToDoItems.Values
      .OrderBy(item => item.Status)
      .ThenByDescending(item => item.CreatedAt)
      .ToArray();

    return Task.FromResult(items);
  }
}
