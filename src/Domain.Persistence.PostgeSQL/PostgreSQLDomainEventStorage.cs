using Flensrocker.Domain.Abstractions;
using Microsoft.Extensions.Logging;

namespace Flensrocker.Domain.Persistence.PostgreSQL;

internal class PostgreSQLDomainEventStorage<TEventBase>(
  PostgreSQLDomainEventStorageOptions<TEventBase> options,
  ILogger<PostgreSQLDomainEventStorage<TEventBase>> logger)
  : IDomainEventStorage<TEventBase>
  where TEventBase : class
{
  private readonly PostgreSQLDomainEventStorageOptions<TEventBase> _options = options;
  private readonly ILogger<PostgreSQLDomainEventStorage<TEventBase>> _logger = logger;

  Task<KeyValuePair<int, TState>> IDomainEventStorage<TEventBase>.ReadStateFromStorage<TState>(Guid streamId, CancellationToken cancellationToken)
  {
    var state = new TState();

    // TODO Read events from database
    DomainEvent<TEventBase>[] domainEvents = [];
    if (domainEvents is { Length: > 0 })
    {
      state.ApplyEvents(domainEvents);
      return Task.FromResult(new KeyValuePair<int, TState>(domainEvents[domainEvents.Length - 1].Metadata.StreamVersion, state));
    }

    return Task.FromResult(new KeyValuePair<int, TState>(0, state));
  }

  Task<bool> IDomainEventStorage<TEventBase>.ApplyUpdatesToStorage(Guid streamId, IReadOnlyList<DomainEvent<TEventBase>> updates, int expectedVersion)
  {
    try
    {
      // TODO Write events to database
      return Task.FromResult(true);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Applying updates to storage for event-base {EventBase}, stream-id {StreamId} and expected version {ExpectedVersion} failed", typeof(TEventBase).GetTypeName(), streamId, expectedVersion);
      return Task.FromResult(false);
    }
  }

  Task<IReadOnlyList<DomainEvent<TEventBase>>?> IDomainEventStorage<TEventBase>.ReadEventsFromStorage(Guid streamId, int fromVersionIncl, int toVersionExcl, CancellationToken cancellationToken)
  {
    if (fromVersionIncl <= 0 || (toVersionExcl > 0 && toVersionExcl <= fromVersionIncl))
      return Task.FromResult<IReadOnlyList<DomainEvent<TEventBase>>?>(null);

    // TODO Read events from database with:
    // StreamVersion >= fromVersionIncl && (toVersionExcl <= 0 || StreamVersion < toVersionExcl)
    IReadOnlyList<DomainEvent<TEventBase>> domainEvents = [];

    if (domainEvents is { Count: > 0 }
      && domainEvents[0].Metadata.StreamVersion == fromVersionIncl
      && (toVersionExcl <= 0 || domainEvents[^1].Metadata.StreamVersion == toVersionExcl - 1))
    {
      return Task.FromResult<IReadOnlyList<DomainEvent<TEventBase>>?>(domainEvents);
    }

    return Task.FromResult<IReadOnlyList<DomainEvent<TEventBase>>?>(null);
  }
}
