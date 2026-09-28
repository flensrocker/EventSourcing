using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Flensrocker.EventSourcing.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Flensrocker.EventSourcing.Persistence.PostgreSQL;

internal partial class PostgreSQLDomainEventStorage<TEventBase, TDbContext>(
  PostgreSQLDomainEventStorageOptions options,
  IDbContextFactory<TDbContext> dbContextFactory,
  ILogger<PostgreSQLDomainEventStorage<TEventBase, TDbContext>> logger
) : IDomainEventStorage<TEventBase>
  where TEventBase : class
  where TDbContext : DbContext
{
  private readonly PostgreSQLDomainEventStorageOptions _options = options;
  private readonly IDbContextFactory<TDbContext> _dbContextFactory = dbContextFactory;
  private readonly ILogger<PostgreSQLDomainEventStorage<TEventBase, TDbContext>> _logger = logger;

  public string StreamType { get; } = options.JsonOptions.GetTypeName(typeof(TEventBase));

  private static readonly string[] _insertColumnList =
  [
    nameof(MessageEntity.Id),
    nameof(MessageEntity.StreamType),
    nameof(MessageEntity.StreamId),
    nameof(MessageEntity.StreamVersion),
    nameof(MessageEntity.TenantId),
    nameof(MessageEntity.CreatedAt),
    nameof(MessageEntity.CreatedByUserId),
    nameof(MessageEntity.TraceId),
    nameof(MessageEntity.SpanId),
    nameof(MessageEntity.PayloadJson),
  ];
  private static readonly string _insertColumnNameList = string.Join(", ", _insertColumnList.Select(n => $"\"{n}\""));

  private static string GetInsertParameterName(int columnIndex, int valueIndex) => $"@{_insertColumnList[columnIndex]}{valueIndex}";

  private static string GetInsertParameterNameList(int index) => $"({string.Join(", ", _insertColumnList.Select(n => $"@{n}{index}"))})";

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
      var parameters = new List<NpgsqlParameter>();
      var insertSql = new StringBuilder();
      insertSql.Append($"INSERT INTO \"{Constants.SchemaName}\".\"{Constants.MessageTableName}\" ({_insertColumnNameList}) VALUES ");

      var valueSql = string.Join(
        ", ",
        updates.Select(
          (ev, valueIndex) =>
          {
            parameters.AddRange(
              new NpgsqlParameter(GetInsertParameterName(0, valueIndex), NpgsqlTypes.NpgsqlDbType.Uuid) { Value = ev.Metadata.EventId },
              new NpgsqlParameter(GetInsertParameterName(1, valueIndex), NpgsqlTypes.NpgsqlDbType.Varchar)
              {
                Value = ev.Metadata.StreamType,
              },
              new NpgsqlParameter(GetInsertParameterName(2, valueIndex), NpgsqlTypes.NpgsqlDbType.Uuid) { Value = ev.Metadata.StreamId },
              new NpgsqlParameter(GetInsertParameterName(3, valueIndex), NpgsqlTypes.NpgsqlDbType.Integer)
              {
                Value = ev.Metadata.StreamVersion,
              },
              new NpgsqlParameter(GetInsertParameterName(4, valueIndex), NpgsqlTypes.NpgsqlDbType.Uuid)
              {
                Value = ev.Metadata.TenantId ?? (object?)DBNull.Value,
              },
              new NpgsqlParameter(GetInsertParameterName(5, valueIndex), NpgsqlTypes.NpgsqlDbType.TimestampTz)
              {
                Value = ev.Metadata.CreatedAt,
              },
              new NpgsqlParameter(GetInsertParameterName(6, valueIndex), NpgsqlTypes.NpgsqlDbType.Uuid)
              {
                Value = ev.Metadata.CreatedByUserId ?? (object?)DBNull.Value,
              },
              new NpgsqlParameter(GetInsertParameterName(7, valueIndex), NpgsqlTypes.NpgsqlDbType.Bytea)
              {
                Value = ev.Metadata.TraceId ?? (object?)DBNull.Value,
              },
              new NpgsqlParameter(GetInsertParameterName(8, valueIndex), NpgsqlTypes.NpgsqlDbType.Bytea)
              {
                Value = ev.Metadata.SpanId ?? (object?)DBNull.Value,
              },
              new NpgsqlParameter(GetInsertParameterName(9, valueIndex), NpgsqlTypes.NpgsqlDbType.Jsonb)
              {
                Value = JsonSerializer.Serialize(ev.Payload, _options.JsonOptions.Options),
              }
            );

            return GetInsertParameterNameList(valueIndex);
          }
        )
      );

      insertSql.Append(valueSql);
      insertSql.Append(';');

      await using var dbContext = await _dbContextFactory.CreateDbContextAsync().ConfigureAwait(false);
      await using var transaction = await dbContext.Database.BeginTransactionAsync().ConfigureAwait(false);

      var insertedCount = await dbContext.Database.ExecuteSqlRawAsync(insertSql.ToString(), parameters).ConfigureAwait(false);

      if (insertedCount != updates.Count)
      {
        await transaction.RollbackAsync().ConfigureAwait(false);
        LogInsertCountError(_logger, StreamType, streamId, expectedVersion, insertedCount, updates.Count);

        return false;
      }

      // TODO execute strong consistent projections retrieved from DI

      await transaction.CommitAsync().ConfigureAwait(false);
      return true;
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

  [LoggerMessage(
    LogLevel.Error,
    Message = "Failure in applying updates to storage for events of type {StreamType}, stream-id {StreamId} and expected version {ExpectedVersion}"
  )]
  internal static partial void LogInsertError(ILogger logger, Exception ex, string streamType, Guid streamId, int expectedVersion);
}
