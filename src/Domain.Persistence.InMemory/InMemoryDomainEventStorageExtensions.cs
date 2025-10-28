using System.Text.Json;
using Flensrocker.Domain.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Flensrocker.Domain.Persistence.InMemory;

public static class InMemoryDomainEventStorageExtensions
{
  // PostgreSQL => Sequence
  internal static long GlobalIndex = 0L;

  internal static DomainEventEntity SetDomainEvent<TEventBase>(this DomainEventEntity entity, DomainEvent<TEventBase> @event, JsonSerializerOptions? jsonOptions)
    where TEventBase : class
  {
    entity.Id = @event.Metadata.EventId;
    entity.StreamType = @event.Metadata.StreamType;
    entity.StreamId = @event.Metadata.StreamId;
    entity.StreamVersion = @event.Metadata.StreamVersion;
    entity.TenantId = @event.Metadata.TenantId;
    entity.CreatedAt = @event.Metadata.CreatedAt;
    entity.CreatedByUserId = @event.Metadata.CreatedByUserId;
    entity.EventType = @event.GetType().GetTypeName();
    entity.EventJson = JsonSerializer.Serialize(@event, jsonOptions);

    return entity;
  }

  internal static DomainEvent<TEventBase> GetDomainEvent<TEventBase>(this DomainEventEntity entity, JsonSerializerOptions? jsonOptions)
    where TEventBase : class
  {
    if (string.IsNullOrWhiteSpace(entity.EventType)
      || Type.GetType(entity.EventType) is not Type type
      || JsonSerializer.Deserialize(entity.EventJson, type, jsonOptions) is not DomainEvent<TEventBase> @event)
      return default!;

    return @event;
  }

  public static IServiceCollection AddInMemoryDomainEventStorage<TEventBase>(this IServiceCollection services, Func<InMemoryDomainEventStorageOptions<TEventBase>> configureOptions)
    where TEventBase : class
  {
    var options = configureOptions() ?? new InMemoryDomainEventStorageOptions<TEventBase>();
    services.AddSingleton(options);

    services.AddScoped<InMemoryDomainEventStorage<TEventBase>>();
    services.AddScoped<IDomainEventStorage<TEventBase>>(sp => sp.GetRequiredService<InMemoryDomainEventStorage<TEventBase>>());

    return services;
  }
}
