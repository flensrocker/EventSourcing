using System.Reactive.Linq;
using System.Reactive.Subjects;
using Flensrocker.EventSourcing.Abstractions;
using Microsoft.Extensions.Logging;
using Orleans.Streams;

namespace Flensrocker.EventSourcing.Orleans.Server;

public partial class DomainEventProjectionObserver<TEventBase, TProjection> : IAsyncObserver<DomainEvent>, IDisposable
  where TEventBase : class
  where TProjection : class, IDomainEventProjectable, new()
{
  private readonly IDomainEventStorage<TEventBase> _eventStore;
  private readonly Guid _projectionId;
  private readonly ILogger _logger;

  private Subject<TProjection> _eventStream = new();

  public TProjection Projection { get; private set; } = default!;

  private DomainEventProjectionObserver(IDomainEventStorage<TEventBase> eventStore, Guid projectionId, ILogger logger)
  {
    _eventStore = eventStore;
    _projectionId = projectionId;
    _logger = logger;
  }

  public static async Task<DomainEventProjectionObserver<TEventBase, TProjection>?> Create(
    IDomainEventStorage<TEventBase> eventStore,
    Guid projectionId,
    ILogger logger,
    CancellationToken cancellationToken
  )
  {
    var observer = new DomainEventProjectionObserver<TEventBase, TProjection>(eventStore, projectionId, logger);

    (var version, observer.Projection) = await eventStore.ReadStateFromStorage<TProjection>(projectionId, cancellationToken);

    if (version == 0)
    {
      observer.Dispose();
      return null;
    }

    return observer;
  }

  public async Task OnNextAsync(DomainEvent item, StreamSequenceToken? token = null)
  {
    if (Projection is IDomainEventVersionable versionable)
    {
      if (item.Metadata.StreamVersion <= versionable.Version)
        return;

      if (item.Metadata.StreamVersion > versionable.Version + 1)
      {
        var missingEvents = await _eventStore.ReadEventsFromStorage(
          _projectionId,
          versionable.Version + 1,
          item.Metadata.StreamVersion,
          default
        );
        if (missingEvents is { Count: > 0 })
          Projection.ApplyEvents(missingEvents);
      }
    }

    Projection.Apply(item);

    _eventStream.OnNext(Projection);
  }

  public Task OnErrorAsync(Exception ex)
  {
    LogObserverError(_logger, ex);
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

  [LoggerMessage(LogLevel.Error, Message = "Observer Error")]
  internal static partial void LogObserverError(ILogger logger, Exception ex);
}
