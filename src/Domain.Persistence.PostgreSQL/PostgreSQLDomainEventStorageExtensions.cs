using System.Reflection;
using System.Text.Json;
using Flensrocker.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;

namespace Flensrocker.Domain.Persistence.PostgreSQL;

public static class PostgreSQLDomainEventStorageExtensions
{
  public class PostgreSQLDomainEventStorageBuilder
  {
    private readonly IServiceCollection _services;

    internal PostgreSQLDomainEventStorageBuilder(IServiceCollection services)
    {
      _services = services;
    }

    public PostgreSQLDomainEventStorageBuilder ForDomainEvents<TEventBase>()
      where TEventBase : class
    {
      _services.AddScoped<PostgreSQLDomainEventStorage<TEventBase>>();
      _services.AddScoped<IDomainEventStorage<TEventBase>>(sp => sp.GetRequiredService<PostgreSQLDomainEventStorage<TEventBase>>());

      return this;
    }
  }

  public static PostgreSQLDomainEventStorageBuilder AddPostgreSQLDomainEventStorage(this IServiceCollection services, Func<PostgreSQLDomainEventStorageOptions> configureOptions)
  {
    var storageOptions = configureOptions();
    services.TryAddSingleton(storageOptions);

    services.AddPooledDbContextFactory<DomainDbContext>((serviceProvider, dbOptions) =>
    {
      var rootConfiguration = serviceProvider.GetRequiredService<IConfiguration>();
      var connectionString = rootConfiguration.GetConnectionString(storageOptions.ConnectionName);

      dbOptions.UseNpgsql(connectionString, npgsqlOptions =>
      {
        npgsqlOptions.MigrationsAssembly(Assembly.GetExecutingAssembly());
        npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", Constants.SchemaName);
        npgsqlOptions.EnableRetryOnFailure();
      });
    });

    return new PostgreSQLDomainEventStorageBuilder(services);
  }

  public static IServiceCollection AddPostgreSQLDomainMessageSubscriber(this IServiceCollection services)
  {
    services.AddHostedService<PostgreSQLDomainMessageSubscriber>();

    return services;
  }

  public static async Task MigratePostgreSQLDomainEventStorageAsync(this IServiceProvider services)
  {
    var dbContextFactory = services.GetRequiredService<IDbContextFactory<DomainDbContext>>();

    using (var migrationDbContext = dbContextFactory.CreateDbContext())
      await migrationDbContext.Database.MigrateAsync().ConfigureAwait(false);

    using (var slotDbContext = dbContextFactory.CreateDbContext())
    {
      var createSlot = $"""
        DO $$
        BEGIN
          SELECT * FROM pg_create_logical_replication_slot('{Constants.PublicationSlotName}', 'pgoutput');
          EXCEPTION WHEN OTHERS THEN NULL;
        END$$;
        """;

      await slotDbContext.Database.ExecuteSqlRawAsync(createSlot).ConfigureAwait(false);
    }
  }

  internal static MessageEntity ToDomainEvent<TEventBase>(this DomainEvent<TEventBase> @event, JsonSerializerOptions? jsonOptions)
    where TEventBase : class
  {
    return new MessageEntity
    {
      Id = @event.Metadata.EventId,
      StreamType = @event.Metadata.StreamType,
      StreamId = @event.Metadata.StreamId,
      StreamVersion = @event.Metadata.StreamVersion,
      TenantId = @event.Metadata.TenantId,
      CreatedAt = @event.Metadata.CreatedAt,
      CreatedByUserId = @event.Metadata.CreatedByUserId,
      PayloadJson = JsonSerializer.Serialize(@event.Payload, jsonOptions),
    };
  }

  internal static DomainEvent<TEventBase> ToDomainEvent<TEventBase>(this MessageEntity entity, JsonSerializerOptions? jsonOptions)
    where TEventBase : class
  {
    return new DomainEvent<TEventBase>
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
      },
      Payload = JsonSerializer.Deserialize(entity.PayloadJson, typeof(TEventBase), jsonOptions) is TEventBase p ? p : default!,
    };
  }
}
