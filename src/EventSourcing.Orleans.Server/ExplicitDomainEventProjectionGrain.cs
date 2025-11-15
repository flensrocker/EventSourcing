using Flensrocker.EventSourcing.Abstractions;
using Flensrocker.EventSourcing.Orleans.Abstractions;
using Flensrocker.EventSourcing.Orleans.Server;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Streams;

namespace Flensrocker.ToDo.Orleans.Server;

public abstract class ExplicitDomainEventProjectionGrain<TEventBase, TProjection> : Grain, IDomainEventProjectionGrain<TProjection>
  where TEventBase : class
  where TProjection : class, IDomainEventProjectable<TEventBase>, new()
{
  private readonly string _streamType = typeof(TEventBase).GetTypeName();
  private readonly IDomainEventStorage<TEventBase> _eventStore;

  private DomainEventProjection<TEventBase, TProjection>? _projection;
  private StreamSubscriptionHandle<DomainEvent>? _subscription;

  public ExplicitDomainEventProjectionGrain()
  {
    _eventStore = ServiceProvider.GetRequiredService<IDomainEventStorage<TEventBase>>();
  }

  private async Task<DomainEventProjection<TEventBase, TProjection>?> GetOrCreateProjection(CancellationToken cancellationToken)
  {
    _projection ??= await DomainEventProjection<TEventBase, TProjection>.Create(_eventStore, this.GetPrimaryKey(), cancellationToken);
    return _projection;
  }

  public override async Task OnActivateAsync(CancellationToken cancellationToken)
  {
    var streamProvider = this.GetStreamProvider(DomainConstants.DomainEventStreamProviderName);
    var streamId = StreamId.Create(_streamType, this.GetPrimaryKey());
    var stream = streamProvider.GetStream<DomainEvent>(streamId);

    var projection = await GetOrCreateProjection(cancellationToken);
    if (projection is not null)
      _subscription = await stream.SubscribeAsync(projection);
  }

  public override async Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
  {
    _projection = null;

    if (_subscription is not null)
    {
      await _subscription.UnsubscribeAsync();
      _subscription = null;
    }
  }

  public async Task<TProjection?> GetProjection(CancellationToken cancellationToken)
  {
    var projection = await GetOrCreateProjection(cancellationToken);
    if (projection is not null)
      return projection.Projection;

    DeactivateOnIdle();
    return default;
  }
}
