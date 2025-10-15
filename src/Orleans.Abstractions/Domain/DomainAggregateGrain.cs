using Flensrocker.Domain.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Orleans.EventSourcing;
using Orleans.EventSourcing.CustomStorage;

namespace Flensrocker.Orleans.Domain.Abstractions;

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
      Metadata = new DomainEventMetadata
      {
        EventId = Guid.CreateVersion7(),
        StreamType = typeof(TEventBase).FullName ?? "",
        StreamId = this.GetPrimaryKey(),
        StreamVersion = _nextVersion,
        TenantId = tenantId,
        CreatedAt = _timeProvider.GetUtcNow(), // ignoring the clock skew in a cluster, the exact time stamp is not important
        CreatedByUserId = userId,
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
