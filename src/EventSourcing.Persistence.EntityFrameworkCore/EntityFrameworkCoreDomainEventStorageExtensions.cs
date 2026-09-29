using System.Text.Json;
using Flensrocker.EventSourcing.Abstractions;
using Flensrocker.EventSourcing.Abstractions.PersistentProjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Flensrocker.EventSourcing.Persistence.EntityFrameworkCore;

public static class EntityFrameworkCoreDomainEventStorageExtensions
{
  extension(IServiceCollection services)
  {
    public IServiceCollection AddDomainEventStorageDbProvider<TDbProvider>()
      where TDbProvider : class, IDomainEventStorageDbProvider
    {
      services.AddScoped<TDbProvider>();
      services.AddScoped<IDomainEventStorageDbProvider>(sp => sp.GetRequiredService<TDbProvider>());

      return services;
    }

    public EntityFrameworkCoreDomainEventStorageBuilder AddDomainEventStorage<TDbProvider>(
      Func<EntityFrameworkCoreDomainEventStorageOptions> configureOptions,
      Func<EventSourcingDbContextOptions> configureDbOptions,
      Action<string?, DbContextOptionsBuilder> configureDbContext
    )
      where TDbProvider : class, IDomainEventStorageDbProvider
    {
      var storageOptions = configureOptions();
      services.TryAddSingleton(storageOptions);

      services.AddDomainEventStorageDbProvider<TDbProvider>();

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

      return new EntityFrameworkCoreDomainEventStorageBuilder(services).ForDomainEvents<
        PersistentProjectionEvent,
        EventSourcingDbContext
      >();
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
