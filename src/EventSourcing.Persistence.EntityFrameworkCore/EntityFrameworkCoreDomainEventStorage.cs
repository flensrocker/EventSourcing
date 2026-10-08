using System.Runtime.CompilerServices;
using Flensrocker.EventSourcing.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Flensrocker.EventSourcing.Persistence.EntityFrameworkCore;

internal partial class EntityFrameworkCoreDomainEventStorage<TEventBase, TDbContext>(
  EntityFrameworkCoreDomainEventStorageOptions options,
  IDomainEventStorageDbProvider dbProvider,
  IDbContextFactory<TDbContext> dbContextFactory,
  IEnumerable<IDbStrongConsistentProjection<TEventBase, TDbContext>> strongConsistentProjections,
  ILogger<EntityFrameworkCoreDomainEventStorage<TEventBase, TDbContext>> logger
) : IDomainEventStorage<TEventBase>
  where TEventBase : class
  where TDbContext : DbContext
{
  private readonly EntityFrameworkCoreDomainEventStorageOptions _options = options;
  private readonly IDomainEventStorageDbProvider _dbProvider = dbProvider;
  private readonly IDbContextFactory<TDbContext> _dbContextFactory = dbContextFactory;
  private readonly IEnumerable<IDbStrongConsistentProjection<TEventBase, TDbContext>> _strongConsistentProjections =
    strongConsistentProjections;
  private readonly ILogger<EntityFrameworkCoreDomainEventStorage<TEventBase, TDbContext>> _logger = logger;

  public string StreamType { get; } = options.JsonOptions.GetTypeName(typeof(TEventBase));

  async Task<KeyValuePair<int, TState>> IDomainEventStorage<TEventBase>.ReadStateFromStorage<TState>(
    Guid streamId,
    CancellationToken cancellationToken
  )
  {
    var state = new TState();

    await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

    var domainEventList = await dbContext
      .Set<MessageEntity>()
      .AsNoTracking()
      .Where(e => e.StreamType == StreamType && e.StreamId == streamId && e.StreamVersion > 0)
      .OrderBy(e => e.StreamVersion)
      .Select(e => e.ToDomainEvent<TEventBase>(_options.JsonOptions.Options))
      .ToArrayAsync(cancellationToken)
      .ConfigureAwait(false);

    if (domainEventList is { Length: > 0 })
    {
      state.ApplyEvents(domainEventList);

      return new KeyValuePair<int, TState>(domainEventList[domainEventList.Length - 1].Metadata.StreamVersion, state);
    }

    return new KeyValuePair<int, TState>(0, state);
  }

  async Task<bool> IDomainEventStorage<TEventBase>.ApplyUpdatesToStorage(
    Guid streamId,
    IReadOnlyList<DomainEvent> updates,
    int expectedVersion
  )
  {
    if (updates.Count == 0)
      return true;

    try
    {
      var insertSql = _dbProvider.GenerateInsertSql(updates, _options.JsonOptions.Options);

      await using var strategyDbContext = await _dbContextFactory.CreateDbContextAsync().ConfigureAwait(false);

      return await strategyDbContext
        .Database.CreateExecutionStrategy()
        .ExecuteAsync(async () =>
        {
          await using var dbContext = await _dbContextFactory.CreateDbContextAsync().ConfigureAwait(false);
          await using var transaction = await dbContext.Database.BeginTransactionAsync().ConfigureAwait(false);

          var insertedCount = await dbContext
            .Database.ExecuteSqlRawAsync(insertSql.InsertScript, insertSql.Parameters)
            .ConfigureAwait(false);

          if (insertedCount != updates.Count)
          {
            await transaction.RollbackAsync().ConfigureAwait(false);
            LogInsertCountError(_logger, StreamType, streamId, expectedVersion, insertedCount, updates.Count);

            return false;
          }

          foreach (var strongConsistentProjection in _strongConsistentProjections)
          {
            await strongConsistentProjection.ApplyEvents(this, dbContext, updates).ConfigureAwait(false);
          }

          await transaction.CommitAsync().ConfigureAwait(false);
          return true;
        })
        .ConfigureAwait(false);
    }
    catch (Exception ex)
    {
      LogInsertError(_logger, ex, StreamType, streamId, expectedVersion);

      return false;
    }
  }

  async Task<IReadOnlyList<DomainEvent>?> IDomainEventStorage<TEventBase>.ReadEventsFromStorage(
    Guid streamId,
    int fromVersionIncl,
    int toVersionExcl,
    CancellationToken cancellationToken
  )
  {
    if (fromVersionIncl > 0 && (toVersionExcl <= 0 || toVersionExcl > fromVersionIncl))
    {
      await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

      var domainEventList = await dbContext
        .Set<MessageEntity>()
        .AsNoTracking()
        .Where(e =>
          e.StreamType == StreamType
          && e.StreamId == streamId
          && e.StreamVersion >= fromVersionIncl
          && (toVersionExcl <= 0 || e.StreamVersion < toVersionExcl)
        )
        .OrderBy(e => e.StreamVersion)
        .Select(e => e.ToDomainEvent<TEventBase>(_options.JsonOptions.Options))
        .ToArrayAsync(cancellationToken)
        .ConfigureAwait(false);

      if (
        domainEventList is { Length: > 0 }
        && domainEventList[0].Metadata.StreamVersion == fromVersionIncl
        && (toVersionExcl <= 0 || domainEventList[^1].Metadata.StreamVersion == toVersionExcl - 1)
      )
      {
        return domainEventList;
      }
    }

    return null;
  }

  async IAsyncEnumerable<DomainEvent> IDomainEventStorage<TEventBase>.ReadAllEventsFromStorage(
    [EnumeratorCancellation] CancellationToken cancellationToken
  )
  {
    await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

    var domainEventList = dbContext
      .Set<MessageEntity>()
      .AsNoTracking()
      .Where(e => e.StreamType == StreamType)
      .OrderBy(e => e.GlobalIndex)
      .Select(e => e.ToDomainEvent<TEventBase>(_options.JsonOptions.Options))
      .AsAsyncEnumerable();

    await foreach (var domainEvent in domainEventList.WithCancellation(cancellationToken).ConfigureAwait(false))
      yield return domainEvent;
  }

  [LoggerMessage(
    LogLevel.Error,
    Message = "Failure in applying updates to storage for events of type {StreamType}, stream-id {StreamId} and expected version {ExpectedVersion}: insert only {InsertedCount} instead of {ExpectedCount}"
  )]
  internal static partial void LogInsertCountError(
    ILogger logger,
    string streamType,
    Guid streamId,
    int expectedVersion,
    int insertedCount,
    int expectedCount
  );

  [LoggerMessage(LogLevel.Error, Message = "Failure in applying strong consistent projection of type {ProjectionType}")]
  internal static partial void LogProjectionUpdateError(ILogger logger, Exception ex, Type projectionType);

  [LoggerMessage(
    LogLevel.Error,
    Message = "Failure in applying updates to storage for events of type {StreamType}, stream-id {StreamId} and expected version {ExpectedVersion}"
  )]
  internal static partial void LogInsertError(ILogger logger, Exception ex, string streamType, Guid streamId, int expectedVersion);
}
