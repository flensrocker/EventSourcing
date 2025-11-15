using Flensrocker.EventSourcing.Abstractions;
using Orleans.Streams;

namespace Flensrocker.EventSourcing.Orleans.Server;

public class DomainEventProjection<TEventBase, TProjection> : IAsyncObserver<DomainEvent>
  where TEventBase : class
  where TProjection : class, IDomainEventProjectable<TEventBase>, new()
{
  private readonly IDomainEventStorage<TEventBase> _eventStore;
  private readonly Guid _projectionId;

  public TProjection Projection { get; private set; } = default!;

  private DomainEventProjection(IDomainEventStorage<TEventBase> eventStore, Guid projectionId)
  {
    _eventStore = eventStore;
    _projectionId = projectionId;
  }

  public static async Task<DomainEventProjection<TEventBase, TProjection>?> Create(IDomainEventStorage<TEventBase> eventStore, Guid projectionId, CancellationToken cancellationToken)
  {
    var observer = new DomainEventProjection<TEventBase, TProjection>(eventStore, projectionId);

    (var version, observer.Projection) = await eventStore.ReadStateFromStorage<TProjection>(projectionId, cancellationToken);

    return version == 0
      ? null
      : observer;
  }

  public async Task OnNextAsync(DomainEvent item, StreamSequenceToken? token = null)
  {
    if (Projection is IDomainEventVersionable withVersion)
    {
      if (item.Metadata.StreamVersion <= withVersion.Version)
        return;

      if (item.Metadata.StreamVersion > withVersion.Version + 1)
      {
        var missingEvents = await _eventStore.ReadEventsFromStorage(_projectionId, withVersion.Version + 1, item.Metadata.StreamVersion, default);
        if (missingEvents is { Count: > 0 })
          Projection.ApplyEvents(missingEvents);
      }
    }

    Projection.Apply(item);
  }

  public Task OnErrorAsync(Exception ex)
  {
    // TODO Error-Handling
    return Task.CompletedTask;
  }
}
