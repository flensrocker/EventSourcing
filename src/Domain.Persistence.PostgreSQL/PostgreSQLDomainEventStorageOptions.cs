using System.Text.Json;

namespace Flensrocker.Domain.Persistence.PostgreSQL;

public class PostgreSQLDomainEventStorageOptions<TEventBase>
{
  public required string ConnectionName { get; init; }
  public JsonSerializerOptions? JsonOptions { get; init; }
}
