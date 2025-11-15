using System.Text.Json;

namespace Flensrocker.EventSourcing.Persistence.InMemory;

public class InMemoryDomainEventStorageOptions<TEventBase>
{
  public JsonSerializerOptions? JsonOptions { get; init; }
}
