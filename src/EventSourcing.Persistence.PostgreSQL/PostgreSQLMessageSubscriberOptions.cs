using Flensrocker.EventSourcing.Abstractions;

namespace Flensrocker.EventSourcing.Persistence.PostgreSQL;

public class PostgreSQLMessageSubscriberOptions
{
  public required string ConnectionName { get; init; }
  public required EventSourcingJsonSerializerOptions JsonOptions { get; init; }
}
