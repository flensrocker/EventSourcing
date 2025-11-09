using System.Reflection;
using System.Text.Json;
using Flensrocker.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Flensrocker.Domain.Persistence.PostgreSQL;

public static class PostgreSQLDomainEventStorageExtensions
{
  public static IServiceCollection AddPostgreSQLDomainEventStorage<TEventBase>(this IServiceCollection services, Func<PostgreSQLDomainEventStorageOptions<TEventBase>> configureOptions)
    where TEventBase : class
  {
    var storageOptions = configureOptions();
    services.AddSingleton(storageOptions);

    services.AddDbContextFactory<DomainDbContext>((serviceProvider, dbOptions) =>
    {
      var rootConfiguration = serviceProvider.GetRequiredService<IConfiguration>();
      var connectionString = rootConfiguration.GetConnectionString(storageOptions.ConnectionName);

      dbOptions.UseNpgsql(connectionString, npgsqlOptions =>
      {
        npgsqlOptions.MigrationsAssembly(Assembly.GetExecutingAssembly());
        npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", Constants.SchemaName);
      });
    });

    services.AddScoped<PostgreSQLDomainEventStorage<TEventBase>>();
    services.AddScoped<IDomainEventStorage<TEventBase>>(sp => sp.GetRequiredService<PostgreSQLDomainEventStorage<TEventBase>>());

    return services;
  }

  public static async Task MigratePostgreSQLDomainEventStorageAsync(this IServiceProvider services)
  {
    var dbContextFactory = services.GetRequiredService<IDbContextFactory<DomainDbContext>>();

    using var dbContext = dbContextFactory.CreateDbContext();
    await dbContext.Database.MigrateAsync().ConfigureAwait(false);
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
