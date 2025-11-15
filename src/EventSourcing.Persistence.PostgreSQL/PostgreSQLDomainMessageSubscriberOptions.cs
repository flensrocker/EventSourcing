using System.Text.Json;

namespace Flensrocker.EventSourcing.Persistence.PostgreSQL;

public class PostgreSQLDomainMessageSubscriberOptions
{
  public required string ConnectionName { get; init; }
  public JsonSerializerOptions? JsonOptions { get; init; }
}
