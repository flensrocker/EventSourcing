using System.Reflection;
using System.Text.Json;
using Flensrocker.EventSourcing.Abstractions;
using Flensrocker.EventSourcing.Abstractions.PersistentProjection;
using Flensrocker.EventSourcing.Extensions.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Npgsql.Replication.PgOutput.Messages;

namespace Flensrocker.EventSourcing.Persistence.PostgreSQL;

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

  extension(IServiceCollection services)
  {
    public PostgreSQLDomainEventStorageBuilder AddPostgreSQLDomainEventStorage(Func<PostgreSQLDomainEventStorageOptions> configureOptions)
    {
      var storageOptions = configureOptions();
      services.TryAddSingleton(storageOptions);

      services.AddPooledDbContextFactory<EventSourcingDbContext>((serviceProvider, dbOptions) =>
      {
        var rootConfiguration = serviceProvider.GetRequiredService<IConfiguration>();
        var connectionString = rootConfiguration.GetConnectionString(storageOptions.ConnectionName);

        dbOptions.UseNpgsql(connectionString, npgsqlOptions =>
        {
          npgsqlOptions.MigrationsAssembly(Assembly.GetExecutingAssembly());
          npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", Constants.SchemaName);
          npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 10,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorCodesToAdd: null);
        });
      });
      services.AddDbMigration<PostgreSQLDomainEventStorageDbMigration>();

      return new PostgreSQLDomainEventStorageBuilder(services)
        .ForDomainEvents<PersistentProjectionEvent>();
    }

    public IServiceCollection AddPostgreSQLDomainMessageSubscriber(Func<PostgreSQLMessageSubscriberOptions> configureOptions)
    {
      services.AddSingleton<PostgreSQLMessageSubscriberMetrics>();

      var storageOptions = configureOptions();
      services.TryAddSingleton(storageOptions);

      services.AddHostedService<PostgreSQLMessageSubscriber>();

      return services;
    }
  }

  extension(MigrationBuilder migrationBuilder)
  {
    internal void CreatePublication()
    {
      migrationBuilder.Sql($"CREATE PUBLICATION {Constants.PublicationName} FOR TABLE \"{Constants.SchemaName}\".\"{Constants.MessageTableName}\" WITH (publish = 'insert');");
    }

    internal void DropPublication()
    {
      migrationBuilder.Sql($"DROP PUBLICATION {Constants.PublicationName};");
    }
  }

  internal static DomainEvent ToDomainEvent<TEventBase>(this MessageEntity entity, JsonSerializerOptions? jsonOptions)
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
      Payload = JsonSerializer.Deserialize(entity.PayloadJson, typeof(TEventBase), jsonOptions) is TEventBase p ? p : default!,
    };
  }

  internal static async Task<(DomainEventMetadata Metadata, object Payload)?> ToDomainEventAsync(this InsertMessage insertMessage, JsonSerializerOptions? jsonOptions, ILogger? logger, CancellationToken cancellationToken)
  {
    Guid? eventId = default;
    string? streamType = default;
    Guid? streamId = default;
    int? streamVersion = default;
    Guid? tenantId = default;
    DateTimeOffset? createdAt = default;
    Guid? createdByUserId = default;
    byte[]? traceId = default;
    byte[]? spanId = default;
    string? payloadJson = default;

    await foreach (var column in insertMessage.NewRow.ConfigureAwait(false))
    {
      switch (column.GetFieldName())
      {
        case nameof(MessageEntity.Id):
          {
            eventId = await column.Get<Guid>(cancellationToken).ConfigureAwait(false);
            break;
          }
        case nameof(MessageEntity.GlobalIndex):
          {
            break;
          }
        case nameof(MessageEntity.StreamType):
          {
            streamType = await column.GetTextReader().ReadToEndAsync(cancellationToken).ConfigureAwait(false);
            break;
          }
        case nameof(MessageEntity.StreamId):
          {
            streamId = await column.Get<Guid>(cancellationToken).ConfigureAwait(false);
            break;
          }
        case nameof(MessageEntity.StreamVersion):
          {
            streamVersion = await column.Get<int>(cancellationToken).ConfigureAwait(false);
            break;
          }
        case nameof(MessageEntity.TenantId):
          {
            tenantId = column.IsDBNull ? null : await column.Get<Guid>(cancellationToken).ConfigureAwait(false);
            break;
          }
        case nameof(MessageEntity.CreatedAt):
          {
            createdAt = await column.Get<DateTimeOffset>(cancellationToken).ConfigureAwait(false);
            break;
          }
        case nameof(MessageEntity.CreatedByUserId):
          {
            createdByUserId = column.IsDBNull ? null : await column.Get<Guid>(cancellationToken).ConfigureAwait(false);
            break;
          }
        case nameof(MessageEntity.TraceId):
          {
            if (column.IsDBNull)
              traceId = null;
            else
            {
              traceId = new byte[16];
              await column.GetStream().ReadExactlyAsync(traceId, 0, traceId.Length, cancellationToken).ConfigureAwait(false);
            }
            break;
          }
        case nameof(MessageEntity.SpanId):
          {
            if (column.IsDBNull)
              spanId = null;
            else
            {
              spanId = new byte[8];
              await column.GetStream().ReadExactlyAsync(spanId, 0, spanId.Length, cancellationToken).ConfigureAwait(false);
            }
            break;
          }
        case nameof(MessageEntity.PayloadJson):
          {
            payloadJson = await column.GetTextReader().ReadToEndAsync(cancellationToken).ConfigureAwait(false);
            break;
          }
      }
    }

    if (eventId.HasValue is false
      || string.IsNullOrEmpty(streamType)
      || streamId.HasValue is false
      || streamVersion.HasValue is false
      || createdAt.HasValue is false
      || payloadJson is null)
    {
      logger?.LogError("""
            Some properties are missing:
            EventId: {EventId}
            StreamType: {StreamType}
            StreamId: {StreamId}
            StreamVersion: {StreamVersion}
            CreatedAt: {CreatedAt}
            """,
        eventId,
        streamType,
        streamId,
        streamVersion,
        createdAt);
      return null;
    }

    if (EventSourcingExtensions.GetTypeFromName(streamType) is not Type payloadType)
    {
      logger?.LogError("Missing type of payload in event-id {EventId}: {PayloadType}", eventId, streamType);
      return null;
    }

    if (JsonSerializer.Deserialize(payloadJson, payloadType, jsonOptions) is not object payload)
    {
      logger?.LogError("Payload can not be deserialized on event-id {EventId}, stream {StreamType}/{StreamId}/{StreamVersion}", eventId, streamType, streamId, streamVersion);
      return null;
    }

    var metadata = new DomainEventMetadata
    {
      EventId = eventId.Value,
      StreamType = streamType,
      StreamId = streamId.Value,
      StreamVersion = streamVersion.Value,
      TenantId = tenantId,
      CreatedAt = createdAt.Value,
      CreatedByUserId = createdByUserId,
      TraceId = traceId,
      SpanId = spanId,
    };

    return (metadata, payload);
  }
}
