using System.Text.Json;

namespace Flensrocker.Domain.Persistence.PostgreSQL;

public class PostgreSQLDomainEventStorageOptions<TEventBase>
{
  public JsonSerializerOptions? JsonOptions { get; init; }
}
