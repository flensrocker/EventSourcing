using System.Text.Json;

namespace Flensrocker.EventSourcing.Persistence.PostgreSQL;

public class PostgreSQLMessageSubscriberOptions
{
  public required string ConnectionName { get; init; }
  public JsonSerializerOptions? JsonOptions { get; init; }
}
