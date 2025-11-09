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

    using var strategyDbContext = await _dbContextFactory.CreateDbContextAsync().ConfigureAwait(false);

    var domainEventList = await strategyDbContext.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
    {
      using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

      return (await dbContext.Message
        .Where(e => e.StreamType == _streamType && e.StreamId == streamId && e.StreamVersion > 0)
        .OrderBy(e => e.StreamVersion)
        .ToArrayAsync(cancellationToken)
        .ConfigureAwait(false))
        .Select(e => e.ToDomainEvent<TEventBase>(_options.JsonOptions))
        .ToArray();
    }).ConfigureAwait(false);

    if (domainEventList is { Length: > 0 })
    {
      state.ApplyEvents(domainEventList);

      return new KeyValuePair<int, TState>(domainEventList[domainEventList.Length - 1].Metadata.StreamVersion, state);
    }

    return new KeyValuePair<int, TState>(0, state);
  }

  async Task<bool> IDomainEventStorage<TEventBase>.ApplyUpdatesToStorage(Guid streamId, IReadOnlyList<DomainEvent<TEventBase>> updates, int expectedVersion)
  {
    try
    {
      using var strategyDbContext = await _dbContextFactory.CreateDbContextAsync().ConfigureAwait(false);

      await strategyDbContext.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
      {
        using var dbContext = await _dbContextFactory.CreateDbContextAsync().ConfigureAwait(false);

        var messageList = updates
          .Select(ev => ev.ToDomainEvent(_options.JsonOptions))
          .ToArray();

        await dbContext.Message.AddRangeAsync(messageList).ConfigureAwait(false);
        await dbContext.SaveChangesAsync().ConfigureAwait(false);
      }).ConfigureAwait(false);

      return true;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failure in applying updates to storage for events of type {StreamType}, stream-id {StreamId} and expected version {ExpectedVersion}", typeof(TEventBase).GetTypeName(), streamId, expectedVersion);

      return false;
    }
  }

  async Task<IReadOnlyList<DomainEvent<TEventBase>>?> IDomainEventStorage<TEventBase>.ReadEventsFromStorage(Guid streamId, int fromVersionIncl, int toVersionExcl, CancellationToken cancellationToken)
  {
    if (fromVersionIncl > 0 && (toVersionExcl <= 0 || toVersionExcl > fromVersionIncl))
    {
      using var strategyDbContext = await _dbContextFactory.CreateDbContextAsync().ConfigureAwait(false);

      var domainEventList = await strategyDbContext.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
      {
        using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        return (await dbContext.Message
          .Where(e => e.StreamType == _streamType && e.StreamId == streamId && e.StreamVersion >= fromVersionIncl && (toVersionExcl <= 0 || e.StreamVersion < toVersionExcl))
          .OrderBy(e => e.StreamVersion)
          .ToArrayAsync(cancellationToken)
          .ConfigureAwait(false))
          .Select(e => e.ToDomainEvent<TEventBase>(_options.JsonOptions))
          .ToArray();
      }).ConfigureAwait(false);

      if (domainEventList is { Length: > 0 }
        && domainEventList[0].Metadata.StreamVersion == fromVersionIncl
        && (toVersionExcl <= 0 || domainEventList[^1].Metadata.StreamVersion == toVersionExcl - 1))
      {
        return domainEventList;
      }
    }

    return null;
  }
}
