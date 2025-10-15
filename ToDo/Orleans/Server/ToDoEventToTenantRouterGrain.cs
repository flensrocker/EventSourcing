using Orleans.Streams;
using Orleans.Streams.Core;
using OrleansPlayground.Abstractions.Domain;
using OrleansPlayground.ToDo.Abstractions;
using OrleansPlayground.ToDo.Orleans.Abstractions;

namespace OrleansPlayground.ToDo.Orleans.Server;

[ImplicitStreamSubscription(ToDoNames.ToDoEventStream)]
public class ToDoEventToTenantRouterGrain : Grain, IGrainWithGuidKey, IStreamSubscriptionObserver, IAsyncObserver<DomainEvent<ToDoEvent>>
{
  public async Task OnSubscribed(IStreamSubscriptionHandleFactory handleFactory)
  {
    var handle = handleFactory.Create<DomainEvent<ToDoEvent>>();
    await handle.ResumeAsync(this);
  }

  public async Task OnNextAsync(DomainEvent<ToDoEvent> item, StreamSequenceToken? token = null)
  {
    var tenantId = item.Metadata.TenantId;
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
