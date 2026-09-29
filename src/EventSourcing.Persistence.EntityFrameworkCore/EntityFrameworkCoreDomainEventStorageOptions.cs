using Flensrocker.EventSourcing.Abstractions;

namespace Flensrocker.EventSourcing.Persistence.EntityFrameworkCore;

public class EntityFrameworkCoreDomainEventStorageOptions
{
  public required string ConnectionName { get; init; }
  public required EventSourcingJsonSerializerOptions JsonOptions { get; init; }
}
