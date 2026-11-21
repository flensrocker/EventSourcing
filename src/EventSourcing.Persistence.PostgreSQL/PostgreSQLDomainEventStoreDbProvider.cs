using System.Text;
using System.Text.Json;
using Flensrocker.EventSourcing.Abstractions;
using Flensrocker.EventSourcing.Persistence.EntityFrameworkCore;
using Npgsql;

namespace Flensrocker.EventSourcing.Persistence.PostgreSQL;

internal sealed class PostgreSQLDomainEventStoreDbProvider : IDomainEventStoreDbProvider
{
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

  public (string InsertScript, IEnumerable<object> Parameters) GenerateInsertSql(
    IReadOnlyList<DomainEvent> domainEvents,
    JsonSerializerOptions jsonOptions
  )
  {
    var sqlBuilder = new StringBuilder();
    var sqlParameters = new List<object>();

    sqlBuilder.Append(
      $"INSERT INTO \"{EntityFrameworkCoreConstants.SchemaName}\".\"{EntityFrameworkCoreConstants.MessageTableName}\" ({_insertColumnNameList}) VALUES "
    );

    var valueSql = string.Join(
      ", ",
      domainEvents.Select(
        (ev, valueIndex) =>
        {
          sqlParameters.AddRange(
            new NpgsqlParameter(GetInsertParameterName(0, valueIndex), NpgsqlTypes.NpgsqlDbType.Uuid) { Value = ev.Metadata.EventId },
            new NpgsqlParameter(GetInsertParameterName(1, valueIndex), NpgsqlTypes.NpgsqlDbType.Varchar) { Value = ev.Metadata.StreamType },
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
              Value = JsonSerializer.Serialize(ev.Payload, jsonOptions),
            }
          );

          return GetInsertParameterNameList(valueIndex);
        }
      )
    );

    sqlBuilder.Append(valueSql);
    sqlBuilder.Append(';');

    return (sqlBuilder.ToString(), sqlParameters);
  }
}
