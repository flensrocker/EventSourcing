using Flensrocker.Domain.Abstractions;
using Flensrocker.Domain.Orleans.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Orleans.EventSourcing;
using Orleans.EventSourcing.CustomStorage;
using Orleans.Streams;

namespace Flensrocker.Domain.Orleans.Server;

public abstract class DomainAggregateGrain<TState, TCommandBase, TEventBase>
  : JournaledGrain<TState, DomainEvent<TEventBase>>, ICustomStorageInterface<TState, DomainEvent<TEventBase>>
  , IDomainAggegrateGrain<TCommandBase>
  where TState : class, IDomainAggregate<TCommandBase, TEventBase>, IDomainEventProjectable<TEventBase>, new()
  where TCommandBase : class
  where TEventBase : class
{
  private readonly string _streamType = typeof(TEventBase).GetTypeName();
  private readonly TimeProvider _timeProvider;
  private readonly IDomainEventStorage<TEventBase> _eventStore;

  private int _nextVersion;
  private IAsyncStream<DomainEvent<TEventBase>>? _domainEventStream;

  protected DomainAggregateGrain()
  {
    _timeProvider = ServiceProvider.GetRequiredService<TimeProvider>();
    _eventStore = ServiceProvider.GetRequiredService<IDomainEventStorage<TEventBase>>();
  }

  public override Task OnActivateAsync(CancellationToken cancellationToken)
  {
    _nextVersion = Version + 1;

    if (!string.IsNullOrWhiteSpace(_streamType))
    {
      var streamProvider = this.GetStreamProvider(DomainConstants.StreamProviderDomainMessages);
      var streamId = StreamId.Create(_streamType, this.GetPrimaryKey());
      _domainEventStream = streamProvider.GetStream<DomainEvent<TEventBase>>(streamId);
    }

    return base.OnActivateAsync(cancellationToken);
  }

  protected DomainEvent<TEventBase>[] RaiseDomainEvents<TEvent>(params IEnumerable<TEvent> payloads)
    where TEvent : TEventBase
  {
    var user = DomainRequestContextExtensions.GetUser();
    var tenantId = user?.TenantId;
    var userId = user?.UserId;

    // TODO Enable adding public messages to domain events.
    var domainEvents = payloads.Select(payload => new DomainEvent<TEventBase>
    {
      Metadata = new DomainEventMetadata
      {
        EventId = Guid.CreateVersion7(),
        StreamType = _streamType,
        StreamId = this.GetPrimaryKey(),
        StreamVersion = _nextVersion++,
        TenantId = tenantId,
        CreatedAt = _timeProvider.GetUtcNow(), // ignoring the clock skew in a cluster, the exact time stamp is not important
        CreatedByUserId = userId,
      },
      Payload = payload,
    }).ToArray();

    RaiseEvents(domainEvents);

    return domainEvents;
  }

  public async Task<bool> ExecuteCommand(TCommandBase command)
  {
    var payloads = await State.ExecuteCommandAsync(command);
    if (payloads is not { Length: > 0 })
      return false;

    var domainEvents = RaiseDomainEvents(payloads);
    await ConfirmEvents();

    // TODO Transaction-Gap between stored and published events
    // TODO Extract publishing to stream into some outbox implementation

    if (_domainEventStream is not null)
    {
      await _domainEventStream.OnNextBatchAsync(domainEvents);
      // TODO Identify public messages on domain-events and publish them to their own streams.
    }

    return true;
  }

  Task<bool> ICustomStorageInterface<TState, DomainEvent<TEventBase>>.ApplyUpdatesToStorage(IReadOnlyList<DomainEvent<TEventBase>> updates, int expectedVersion)
    => _eventStore.ApplyUpdatesToStorage(this.GetPrimaryKey(), updates, expectedVersion);

  Task<KeyValuePair<int, TState>> ICustomStorageInterface<TState, DomainEvent<TEventBase>>.ReadStateFromStorage()
    => _eventStore.ReadStateFromStorage<TState>(this.GetPrimaryKey(), default);
}
