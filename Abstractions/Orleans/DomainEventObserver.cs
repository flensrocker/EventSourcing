using Orleans.Streams;
using OrleansPlayground.Abstractions.Domain;
using OrleansPlayground.Abstractions.EntityFrameworkCore;

namespace OrleansPlayground.Abstractions.Orleans;

public class DomainEventObserver<TEventBase, TView> : IAsyncObserver<DomainEvent<TEventBase>>
  where TEventBase : class
  where TView : class, IEventSourceable<TEventBase>, new()
{
  private readonly IDomainEventStore<TEventBase> _eventStore;
  private readonly Guid _viewId;

  public int ViewVersion { get; private set; }
  public TView View { get; private set; } = default!;

  private DomainEventObserver(IDomainEventStore<TEventBase> eventStore, Guid viewId)
  {
    _eventStore = eventStore;
    _viewId = viewId;
  }

  public static async Task<DomainEventObserver<TEventBase, TView>> Create(IDomainEventStore<TEventBase> eventStore, Guid viewId)
  {
    var observer = new DomainEventObserver<TEventBase, TView>(eventStore, viewId);

    (observer.ViewVersion, observer.View) = await eventStore.ReadStateFromStorage<TView>(viewId);

    return observer;
  }

  public async Task OnNextAsync(DomainEvent<TEventBase> item, StreamSequenceToken? token = null)
  {
    if (item.Metadata.EntityVersion <= ViewVersion)
      return;

    if (item.Metadata.EntityVersion > ViewVersion + 1)
    {
      var missingEvents = await _eventStore.ReadEventsFromStorage(_viewId, ViewVersion + 1, item.Metadata.EntityVersion);
      if (missingEvents is { Count: > 0 })
        View.ApplyEvents(missingEvents);
    }

    View.ApplyEvents(item);
    // TODO push updated view somewhere... stream? IAsyncEnumerable?

    return;
  }

  public Task OnErrorAsync(Exception ex)
  {
    // TODO Error-Handling
    return Task.CompletedTask;
  }
}
