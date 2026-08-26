using Flensrocker.EventSourcing.Abstractions;
using Flensrocker.EventSourcing.Orleans.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Orleans.EventSourcing;
using Orleans.EventSourcing.CustomStorage;

namespace Flensrocker.EventSourcing.Orleans.Server;

public abstract class CommandHandlerGrain<TEventBase, TCommandBase, TCommandState>
  : JournaledGrain<TCommandState, DomainEvent>, ICustomStorageInterface<TCommandState, DomainEvent>
  , ICommandHandlerGrain<TCommandBase>
  where TEventBase : class
  where TCommandBase : class
  where TCommandState : class, IDomainEventProjectable, new()
{
  private readonly TimeProvider _timeProvider;
  private readonly IDomainEventStorage<TEventBase> _eventStore;

  private int _nextVersion;

  protected CommandHandlerGrain()
  {
    _timeProvider = ServiceProvider.GetRequiredService<TimeProvider>();
    _eventStore = ServiceProvider.GetRequiredService<IDomainEventStorage<TEventBase>>();
  }

  public override async Task OnActivateAsync(CancellationToken cancellationToken)
  {
    await base.OnActivateAsync(cancellationToken);

    _nextVersion = Version + 1;
  }

  protected DomainEvent[] RaiseDomainEvents<TEvent>(params IEnumerable<TEvent> payloads)
    where TEvent : TEventBase
  {
    var user = OrleansRequestContextExtensions.GetUser();
    var tenantId = user?.TenantId;
    var userId = user?.UserId;

    // TODO Enable adding public messages to domain events.
    var streamId = this.GetPrimaryKey();
    var createdAt = _timeProvider.GetUtcNow();
    var domainEvents = payloads
      .Select((payload, index) => DomainEvent.Create<TEventBase, TEvent>(streamId, _nextVersion + index, payload, tenantId, userId, createdAt))
      .ToArray();
    _nextVersion += domainEvents.Length;

    RaiseEvents(domainEvents);

    return domainEvents;
  }

  public async Task<bool> ExecuteCommand(TCommandBase command)
  {
    var commandHandler = ServiceProvider.GetRequiredService<ICommandHandler<TEventBase, TCommandBase, TCommandState>>();
    var payloads = await commandHandler.ExecuteCommandAsync(State, command);
    if (payloads is not { Length: > 0 })
      return false;

    var domainEvents = RaiseDomainEvents(payloads);
    await ConfirmEvents();

    return true;
  }

  Task<bool> ICustomStorageInterface<TCommandState, DomainEvent>.ApplyUpdatesToStorage(IReadOnlyList<DomainEvent> updates, int expectedVersion)
    => _eventStore.ApplyUpdatesToStorage(this.GetPrimaryKey(), updates, expectedVersion);

  Task<KeyValuePair<int, TCommandState>> ICustomStorageInterface<TCommandState, DomainEvent>.ReadStateFromStorage()
    => _eventStore.ReadStateFromStorage<TCommandState>(this.GetPrimaryKey(), default);
}
