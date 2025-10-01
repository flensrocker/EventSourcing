using Orleans.EventSourcing;
using Orleans.EventSourcing.CustomStorage;
using OrleansPlayground.Abstractions.Domain;
using OrleansPlayground.Abstractions.Persistence;

namespace OrleansPlayground.Abstractions.Orleans;

public abstract class DomainAggregateGrain<TState, TEventBase>
  : JournaledGrain<TState, DomainEvent<TEventBase>>, ICustomStorageInterface<TState, DomainEvent<TEventBase>>
  where TState : class, IDomainEventProjectable<TEventBase>, new()
  where TEventBase : class
{
  private readonly TimeProvider _timeProvider;
  private readonly IDomainEventStorage<TEventBase> _eventStore;
  private int _nextVersion;

  protected DomainAggregateGrain()
  {
    _timeProvider = ServiceProvider.GetRequiredService<TimeProvider>();
    _eventStore = ServiceProvider.GetRequiredService<IDomainEventStorage<TEventBase>>();
  }

  public override Task OnActivateAsync(CancellationToken cancellationToken)
  {
    _nextVersion = Version + 1;
    return base.OnActivateAsync(cancellationToken);
  }

  protected DomainEvent<TEventBase> RaiseDomainEvent<TEvent>(TEvent payload)
    where TEvent : TEventBase
  {
    var user = DomainRequestContextExtensions.GetUser();
    var tenantId = user?.TenantId ?? Guid.Empty;
    var userId = user?.UserId ?? Guid.Empty;

    var domainEvent = new DomainEvent<TEventBase>
    {
      EventId = Guid.CreateVersion7(),
      Metadata = new DomainEventMetadata
      {
        TenantId = tenantId,
        CreatedAt = _timeProvider.GetUtcNow(), // ignoring the clock skew in a cluster, the exact time stamp is not important
        CreatedByUserId = userId,
        EntityType = typeof(TState).FullName ?? "",
        EntityId = this.GetPrimaryKey(),
        EntityVersion = _nextVersion,
      },
      Payload = payload,
    };
    _nextVersion++;

    RaiseEvent(domainEvent);

    return domainEvent;
  }

  public Task<bool> ApplyUpdatesToStorage(IReadOnlyList<DomainEvent<TEventBase>> updates, int expectedVersion)
    => _eventStore.ApplyUpdatesToStorage(this.GetPrimaryKey(), updates, expectedVersion);

  public Task<KeyValuePair<int, TState>> ReadStateFromStorage()
    => _eventStore.ReadStateFromStorage<TState>(this.GetPrimaryKey());
}
