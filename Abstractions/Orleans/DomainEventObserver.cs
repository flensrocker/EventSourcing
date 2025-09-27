using Orleans.Streams;
using OrleansPlayground.Abstractions.Domain;
using OrleansPlayground.Abstractions.Persistence;

namespace OrleansPlayground.Abstractions.Orleans;

public class DomainEventObserver<TEventBase, TView> : IAsyncObserver<DomainEvent<TEventBase>>
  where TEventBase : class
  where TView : class, IDomainEventSourceable<TEventBase>, new()
{
  private readonly IDomainEventStore<TEventBase> _eventStore;
  private readonly Guid _viewId;

  public TView View { get; private set; } = default!;

  private DomainEventObserver(IDomainEventStore<TEventBase> eventStore, Guid viewId)
  {
    _eventStore = eventStore;
    _viewId = viewId;
  }

  public static async Task<DomainEventObserver<TEventBase, TView>> Create(IDomainEventStore<TEventBase> eventStore, Guid viewId)
  {
    var observer = new DomainEventObserver<TEventBase, TView>(eventStore, viewId);

    (_, observer.View) = await eventStore.ReadStateFromStorage<TView>(viewId);

    return observer;
  }

  public Task OnNextAsync(DomainEvent<TEventBase> item)
    => OnNextAsync(item, null);

  public async Task OnNextAsync(DomainEvent<TEventBase> item, StreamSequenceToken? token = null)
  {
    if (item.Metadata.EntityVersion <= View.Version)
      return;

    if (item.Metadata.EntityVersion > View.Version + 1)
    {
      var missingEvents = await _eventStore.ReadEventsFromStorage(_viewId, View.Version + 1, item.Metadata.EntityVersion);
      if (missingEvents is { Count: > 0 })
        View.ApplyEvents(missingEvents);
    }

    View.Apply(item);
    // TODO push updated view somewhere... stream? IAsyncEnumerable?

    return;
  }

  public Task OnErrorAsync(Exception ex)
  {
    // TODO Error-Handling
    return Task.CompletedTask;
  }
}
