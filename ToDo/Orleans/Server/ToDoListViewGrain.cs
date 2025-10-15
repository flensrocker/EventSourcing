using Orleans.Streams;
using Orleans.Streams.Core;
using OrleansPlayground.Abstractions.Domain;
using OrleansPlayground.ToDo.Abstractions;
using OrleansPlayground.ToDo.Orleans.Abstractions;
using OrleansPlayground.ToDo.Views;

namespace OrleansPlayground.ToDo.Orleans.Server;

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
