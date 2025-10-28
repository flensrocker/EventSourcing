using Flensrocker.Domain.Abstractions;
using Orleans.Streams;

namespace Flensrocker.Domain.Orleans.Server;

public class DomainEventProjection<TEventBase, TView> : IAsyncObserver<DomainEvent<TEventBase>>
  where TEventBase : class
  where TView : class, IDomainEventProjectable<TEventBase>, new()
{
  private readonly IDomainEventStorage<TEventBase> _eventStore;
  private readonly Guid _viewId;

  public TView View { get; private set; } = default!;

  private DomainEventProjection(IDomainEventStorage<TEventBase> eventStore, Guid viewId)
  {
    _eventStore = eventStore;
    _viewId = viewId;
  }

  public static async Task<DomainEventProjection<TEventBase, TView>> Create(IDomainEventStorage<TEventBase> eventStore, Guid viewId)
  {
    var observer = new DomainEventProjection<TEventBase, TView>(eventStore, viewId);

    (_, observer.View) = await eventStore.ReadStateFromStorage<TView>(viewId);

    return observer;
  }

  public async Task OnNextAsync(DomainEvent<TEventBase> item, StreamSequenceToken? token = null)
  {
    if (View is IDomainEventVersionable versionedView)
    {
      if (item.Metadata.StreamVersion <= versionedView.Version)
        return;

      if (item.Metadata.StreamVersion > versionedView.Version + 1)
      {
        var missingEvents = await _eventStore.ReadEventsFromStorage(_viewId, versionedView.Version + 1, item.Metadata.StreamVersion);
        if (missingEvents is { Count: > 0 })
          View.ApplyEvents(missingEvents);
      }
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
