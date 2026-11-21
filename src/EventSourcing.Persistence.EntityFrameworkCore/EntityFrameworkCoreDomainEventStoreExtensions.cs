using System.Text.Json;
using Flensrocker.EventSourcing.Abstractions;
using Flensrocker.EventSourcing.Abstractions.PersistentProjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Flensrocker.EventSourcing.Persistence.EntityFrameworkCore;

public static class EntityFrameworkCoreDomainEventStoreExtensions
{
  extension(IServiceCollection services)
  {
    public IServiceCollection AddDomainEventStoreDbProvider<TDbProvider>()
      where TDbProvider : class, IDomainEventStoreDbProvider
    {
      services.AddScoped<TDbProvider>();
      services.AddScoped<IDomainEventStoreDbProvider>(sp => sp.GetRequiredService<TDbProvider>());

      return services;
    }

    public EntityFrameworkCoreDomainEventStoreBuilder AddDomainEventStore<TDbProvider>(
      Func<EntityFrameworkCoreDomainEventStoreOptions> configureOptions,
      Func<EventSourcingDbContextOptions> configureDbOptions,
      Action<string?, DbContextOptionsBuilder> configureDbContext
    )
      where TDbProvider : class, IDomainEventStoreDbProvider
    {
      var storageOptions = configureOptions();
      services.TryAddSingleton(storageOptions);

      services.AddDomainEventStoreDbProvider<TDbProvider>();

      var contextOptions = configureDbOptions();
      services.AddSingleton(contextOptions);
      services.AddPooledDbContextFactory<EventSourcingDbContext>(
        (serviceProvider, dbOptions) =>
        {
          var rootConfiguration = serviceProvider.GetRequiredService<IConfiguration>();
          var connectionString = rootConfiguration.GetConnectionString(storageOptions.ConnectionName);

          configureDbContext(connectionString, dbOptions);
        }
      );

      return new EntityFrameworkCoreDomainEventStoreBuilder(services).ForDomainEvents<PersistentProjectionEvent, EventSourcingDbContext>();
    }
  }

  public static DomainEvent ToDomainEvent<TEventBase>(this MessageEntity entity, JsonSerializerOptions? jsonOptions)
    where TEventBase : class
  {
    return new DomainEvent
    {
      Metadata = new DomainEventMetadata
      {
        EventId = entity.Id,
        StreamType = entity.StreamType,
        StreamId = entity.StreamId,
        StreamVersion = entity.StreamVersion,
        TenantId = entity.TenantId,
        CreatedAt = entity.CreatedAt,
        CreatedByUserId = entity.CreatedByUserId,
        TraceId = entity.TraceId,
        SpanId = entity.SpanId,
      },
      Payload = JsonSerializer.Deserialize<TEventBase>(entity.PayloadJson, jsonOptions) is TEventBase p ? p : default!,
    };
  }
}
