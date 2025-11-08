using Flensrocker.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Flensrocker.Domain.Persistence.PostgreSQL;

internal class PostgreSQLDomainEventStorage<TEventBase>(
  PostgreSQLDomainEventStorageOptions<TEventBase> options,
  IDbContextFactory<DomainDbContext> dbContextFactory,
  ILogger<PostgreSQLDomainEventStorage<TEventBase>> logger)
  : IDomainEventStorage<TEventBase>
  where TEventBase : class
{
  private readonly PostgreSQLDomainEventStorageOptions<TEventBase> _options = options;
  private readonly IDbContextFactory<DomainDbContext> _dbContextFactory = dbContextFactory;
  private readonly ILogger<PostgreSQLDomainEventStorage<TEventBase>> _logger = logger;

  private readonly string _streamType = typeof(TEventBase).GetTypeName();

  async Task<KeyValuePair<int, TState>> IDomainEventStorage<TEventBase>.ReadStateFromStorage<TState>(Guid streamId, CancellationToken cancellationToken)
  {
    var state = new TState();

    DomainEvent<TEventBase>[] domainEvents;
    await using (var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
    {
      domainEvents = (await dbContext.DomainEvent
        .Where(e => e.StreamType == _streamType && e.StreamId == streamId)
        .OrderBy(e => e.StreamVersion)
        .ToArrayAsync(cancellationToken)
        .ConfigureAwait(false))
        .Select(e => e.ToDomainEvent<TEventBase>(_options.JsonOptions))
        .ToArray();
    }

    if (domainEvents is { Length: > 0 })
    {
      state.ApplyEvents(domainEvents);

      return new KeyValuePair<int, TState>(domainEvents[domainEvents.Length - 1].Metadata.StreamVersion, state);
    }

    return new KeyValuePair<int, TState>(0, state);
  }

  async Task<bool> IDomainEventStorage<TEventBase>.ApplyUpdatesToStorage(Guid streamId, IReadOnlyList<DomainEvent<TEventBase>> updates, int expectedVersion)
  {
    try
    {
      await using (var dbContext = await _dbContextFactory.CreateDbContextAsync().ConfigureAwait(false))
      {
        var entites = updates
          .Select(ev => ev.ToDomainEvent(_options.JsonOptions))
          .ToArray();

        await dbContext.DomainEvent.AddRangeAsync(entites).ConfigureAwait(false);
        await dbContext.SaveChangesAsync().ConfigureAwait(false);
      }

      return true;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Applying updates to storage for event-base {EventBase}, stream-id {StreamId} and expected version {ExpectedVersion} failed", typeof(TEventBase).GetTypeName(), streamId, expectedVersion);

      return false;
    }
  }

  async Task<IReadOnlyList<DomainEvent<TEventBase>>?> IDomainEventStorage<TEventBase>.ReadEventsFromStorage(Guid streamId, int fromVersionIncl, int toVersionExcl, CancellationToken cancellationToken)
  {
    if (fromVersionIncl > 0 && (toVersionExcl <= 0 || toVersionExcl > fromVersionIncl))
    {
      DomainEvent<TEventBase>[] domainEvents;
      await using (var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
      {
        domainEvents = (await dbContext.DomainEvent
          .Where(e => e.StreamType == _streamType && e.StreamId == streamId && e.StreamVersion >= fromVersionIncl && (toVersionExcl <= 0 || e.StreamVersion < toVersionExcl))
          .OrderBy(e => e.StreamVersion)
          .ToArrayAsync(cancellationToken)
          .ConfigureAwait(false))
          .Select(e => e.ToDomainEvent<TEventBase>(_options.JsonOptions))
          .ToArray();
      }

      if (domainEvents is { Length: > 0 }
        && domainEvents[0].Metadata.StreamVersion == fromVersionIncl
        && (toVersionExcl <= 0 || domainEvents[^1].Metadata.StreamVersion == toVersionExcl - 1))
      {
        return domainEvents;
      }
    }

    return null;
  }
}
