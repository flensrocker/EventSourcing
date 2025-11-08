using Flensrocker.Domain.Abstractions;
using Flensrocker.Domain.Orleans.Abstractions;
using Flensrocker.ToDo.Abstractions;
using Flensrocker.ToDo.Orleans.Abstractions;
using Orleans.Streams;
using Orleans.Streams.Core;

namespace Flensrocker.ToDo.Orleans.Server;

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
    if (tenantId.HasValue is false)
      return;

    var tenantStreamId = StreamId.Create(ToDoNames.ToDoListViewStream, tenantId.Value);
    var tenantStream = this.GetStreamProvider(DomainConstants.StreamProviderDomainEvents).GetStream<DomainEvent<ToDoEvent>>(tenantStreamId);

    await tenantStream.OnNextAsync(item);
  }

  public Task OnErrorAsync(Exception ex)
  {
    // TODO add error handling
    return Task.CompletedTask;
  }
}
