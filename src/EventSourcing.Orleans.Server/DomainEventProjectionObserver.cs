using System.Reactive.Linq;
using System.Reactive.Subjects;
using Flensrocker.EventSourcing.Abstractions;
using Orleans.Streams;

namespace Flensrocker.EventSourcing.Orleans.Server;

public class DomainEventProjectionObserver<TEventBase, TProjection> : IAsyncObserver<DomainEvent>, IDisposable
  where TEventBase : class
  where TProjection : class, IDomainEventProjectable, new()
{
  private readonly IDomainEventStorage<TEventBase> _eventStore;
  private readonly Guid _projectionId;

  private Subject<TProjection> _eventStream = new();

  public TProjection Projection { get; private set; } = default!;

  private DomainEventProjectionObserver(IDomainEventStorage<TEventBase> eventStore, Guid projectionId)
  {
    _eventStore = eventStore;
    _projectionId = projectionId;
  }

  public static async Task<DomainEventProjectionObserver<TEventBase, TProjection>?> Create(IDomainEventStorage<TEventBase> eventStore, Guid projectionId, CancellationToken cancellationToken)
  {
    var observer = new DomainEventProjectionObserver<TEventBase, TProjection>(eventStore, projectionId);

    (var version, observer.Projection) = await eventStore.ReadStateFromStorage<TProjection>(projectionId, cancellationToken);

    return version == 0
      ? null
      : observer;
  }

  public async Task OnNextAsync(DomainEvent item, StreamSequenceToken? token = null)
  {
    if (Projection is IDomainEventVersionable versionable)
    {
      if (item.Metadata.StreamVersion <= versionable.Version)
        return;

      if (item.Metadata.StreamVersion > versionable.Version + 1)
      {
        var missingEvents = await _eventStore.ReadEventsFromStorage(_projectionId, versionable.Version + 1, item.Metadata.StreamVersion, default);
        if (missingEvents is { Count: > 0 })
          Projection.ApplyEvents(missingEvents);
      }
    }

    Projection.Apply(item);

    _eventStream.OnNext(Projection);
  }

  public Task OnErrorAsync(Exception ex)
  {
    // TODO Error-Handling
    return Task.CompletedTask;
  }

  public IObservable<TProjection> AsObservable()
  {
    return _eventStream.AsObservable();
  }

  public void Dispose()
  {
    _eventStream.Dispose();
  }
}
