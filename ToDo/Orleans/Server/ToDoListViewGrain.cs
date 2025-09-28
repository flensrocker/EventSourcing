using Orleans.Streams;
using Orleans.Streams.Core;
using OrleansPlayground.Abstractions.Domain;
using OrleansPlayground.ToDo.Domain;
using OrleansPlayground.ToDo.Orleans.Abstractions;
using OrleansPlayground.ToDo.Views;

namespace OrleansPlayground.ToDo.Orleans.Server;

[ImplicitStreamSubscription(ToDoNames.ToDoEventStream)]
public class ToDoEventRouterGrain : Grain, IGrainWithGuidKey, IStreamSubscriptionObserver, IAsyncObserver<DomainEvent<ToDoEvent>>
{
  public async Task OnSubscribed(IStreamSubscriptionHandleFactory handleFactory)
  {
    var handle = handleFactory.Create<DomainEvent<ToDoEvent>>();
    await handle.ResumeAsync(this);
  }

  public async Task OnNextAsync(DomainEvent<ToDoEvent> item, StreamSequenceToken? token = null)
  {
    var tenantId = Guid.Empty; // TODO derive from item
    var tenantStreamId = StreamId.Create(ToDoNames.ToDoListViewStream, tenantId);
    var tenantStream = this.GetStreamProvider(ToDoNames.ToDoEventStreamProvider).GetStream<DomainEvent<ToDoEvent>>(tenantStreamId);

    await tenantStream.OnNextAsync(item);
  }

  public Task OnErrorAsync(Exception ex)
  {
    // TODO add error handling
    return Task.CompletedTask;
  }
}

[ImplicitStreamSubscription(ToDoNames.ToDoListViewStream)]
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
