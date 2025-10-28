using System.Text.Json;

namespace Flensrocker.Domain.Persistence.InMemory;

public class InMemoryDomainEventStorageOptions<TEventBase>
{
  public JsonSerializerOptions? JsonOptions { get; init; }
}
