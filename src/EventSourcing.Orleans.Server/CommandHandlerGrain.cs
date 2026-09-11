using Flensrocker.EventSourcing.Abstractions;
using Flensrocker.EventSourcing.Orleans.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Orleans.EventSourcing;
using Orleans.EventSourcing.CustomStorage;

namespace Flensrocker.EventSourcing.Orleans.Server;

public abstract class CommandHandlerGrain<TEventBase, TCommandBase, TCommandState>
  : JournaledGrain<TCommandState, DomainEvent>,
    ICustomStorageInterface<TCommandState, DomainEvent>,
    ICommandHandlerGrain<TCommandBase>
  where TEventBase : class
  where TCommandBase : class
  where TCommandState : class, IDomainEventProjectable, new()
{
  private readonly IDomainEventStorage<TEventBase> _eventStore;

  private int _nextVersion;

  protected CommandHandlerGrain()
  {
    _eventStore = ServiceProvider.GetRequiredService<IDomainEventStorage<TEventBase>>();
  }

  public override async Task OnActivateAsync(CancellationToken cancellationToken)
  {
    await base.OnActivateAsync(cancellationToken);

    _nextVersion = Version + 1;
  }

  protected void RaiseDomainEvents(DateTimeOffset createdAt, TEventBase[] payloads)
  {
    var user = OrleansRequestContextExtensions.GetUser();
    var tenantId = user?.TenantId;
    var userId = user?.UserId;

    // TODO Enable adding public messages to domain events.
    var streamId = this.GetPrimaryKey();

    if (payloads.Length == 1)
    {
      var domainEvent = DomainEvent.Create(_eventStore.StreamType, streamId, _nextVersion, payloads[0], tenantId, userId, createdAt);
      _nextVersion++;

      RaiseEvent(domainEvent);
    }
    else
    {
      var domainEventList = new List<DomainEvent>(payloads.Length);
      foreach (var payload in payloads)
      {
        var domainEvent = DomainEvent.Create(_eventStore.StreamType, streamId, _nextVersion, payload, tenantId, userId, createdAt);
        _nextVersion++;

        domainEventList.Add(domainEvent);
      }

      RaiseEvents(domainEventList);
    }
  }

  public async Task<bool> ExecuteCommand(TCommandBase command)
  {
    await using var commandScope = ServiceProvider.CreateAsyncScope();

    var commandHandler = commandScope.ServiceProvider.GetRequiredService<ICommandHandler<TEventBase, TCommandBase, TCommandState>>();

    var payloadsResult = await commandHandler.ExecuteCommandAsync(State, command);
    if (!payloadsResult.IsSuccess(out var payloads) || payloads.Length == 0)
      return false;

    var timeProvider = commandScope.ServiceProvider.GetRequiredService<TimeProvider>();
    var createdAt = timeProvider.GetUtcNow();

    RaiseDomainEvents(createdAt, payloads);
    await ConfirmEvents();

    return true;
  }

  Task<bool> ICustomStorageInterface<TCommandState, DomainEvent>.ApplyUpdatesToStorage(
    IReadOnlyList<DomainEvent> updates,
    int expectedVersion
  ) => _eventStore.ApplyUpdatesToStorage(this.GetPrimaryKey(), updates, expectedVersion);

  Task<KeyValuePair<int, TCommandState>> ICustomStorageInterface<TCommandState, DomainEvent>.ReadStateFromStorage() =>
    _eventStore.ReadStateFromStorage<TCommandState>(this.GetPrimaryKey(), default);
}
