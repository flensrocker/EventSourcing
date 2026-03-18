using System.Runtime.CompilerServices;
using Flensrocker.EventSourcing.Abstractions;
using Flensrocker.EventSourcing.Orleans.Abstractions;
using Flensrocker.EventSourcing.Orleans.Server;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Streams;

namespace Flensrocker.ToDo.Orleans.Server;

public abstract class ExplicitDomainEventProjectionGrain<TEventBase, TProjection> : Grain, IDomainEventProjectionGrain<TProjection>, IDisposable
  where TEventBase : class
  where TProjection : class, IDomainEventProjectable, new()
{
  private readonly IDomainEventStorage<TEventBase> _eventStore;

  private DomainEventProjectionObserver<TEventBase, TProjection>? _projection;
  private StreamSubscriptionHandle<DomainEvent>? _subscription;

  protected ExplicitDomainEventProjectionGrain()
  {
    _eventStore = ServiceProvider.GetRequiredService<IDomainEventStorage<TEventBase>>();
  }

  private async Task<DomainEventProjectionObserver<TEventBase, TProjection>?> GetOrCreateProjection(CancellationToken cancellationToken)
  {
    _projection ??= await DomainEventProjectionObserver<TEventBase, TProjection>.Create(_eventStore, this.GetPrimaryKey(), cancellationToken);
    return _projection;
  }

  public override async Task OnActivateAsync(CancellationToken cancellationToken)
  {
    var projection = await GetOrCreateProjection(cancellationToken);

    if (projection is not null)
    {
      var streamType = typeof(TEventBase).GetTypeName();
      var streamKey = this.GetPrimaryKey();
      var streamProvider = this.GetStreamProvider(OrleansConstants.DomainEventStreamProviderName);
      var streamId = StreamId.Create(streamType, streamKey);
      var stream = streamProvider.GetStream<DomainEvent>(streamId);

      _subscription = await stream.SubscribeAsync(projection);
    }
  }

  public override async Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
  {
    if (_subscription is not null)
    {
      await _subscription.UnsubscribeAsync();
      _subscription = null;
    }

    _projection?.Dispose();
    _projection = null;
  }

  public async Task<TProjection?> GetProjection(CancellationToken cancellationToken)
  {
    var projection = await GetOrCreateProjection(cancellationToken);

    if (projection is null)
      DeactivateOnIdle();

    return projection?.Projection;
  }

  public async IAsyncEnumerable<TProjection> StreamProjection([EnumeratorCancellation] CancellationToken cancellationToken)
  {
    var projection = await GetOrCreateProjection(cancellationToken);
    if (projection is null)
    {
      DeactivateOnIdle();
      yield break;
    }

    yield return projection.Projection;

    await foreach (var value in projection.AsObservable().ToAsyncEnumerable().WithCancellation(cancellationToken))
      yield return value;
  }

  public void Dispose()
  {
    _projection?.Dispose();
  }
}
