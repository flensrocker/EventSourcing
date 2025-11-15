using System.Text.Json;
using Flensrocker.Domain.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql.Replication;
using Npgsql.Replication.PgOutput;
using Npgsql.Replication.PgOutput.Messages;

namespace Flensrocker.Domain.Persistence.PostgreSQL;

internal class PostgreSQLDomainMessageSubscriber(
  IConfiguration configuration,
  PostgreSQLDomainMessageSubscriberOptions options,
  ILogger<PostgreSQLDomainMessageSubscriber> logger)
  : BackgroundService
{
  private readonly IConfiguration _configuration = configuration;
  private readonly PostgreSQLDomainMessageSubscriberOptions _options = options;
  private readonly ILogger<PostgreSQLDomainMessageSubscriber> _logger = logger;

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    var connectionString = _configuration.GetConnectionString(_options.ConnectionName);

    // TODO add error handling
    await using var conn = new LogicalReplicationConnection(connectionString);
    await conn.Open(stoppingToken).ConfigureAwait(false);

    var slot = new PgOutputReplicationSlot(Constants.PublicationSlotName);

    await foreach (var message in conn.StartReplication(slot, new PgOutputReplicationOptions(Constants.PublicationName, PgOutputProtocolVersion.V4, binary: true), stoppingToken).ConfigureAwait(false))
    {
      Console.WriteLine($"Received message type: {message.GetType().Name}");

      if (message is InsertMessage insertMessage)
      {
        Guid eventId = default;
        long globalIndex = default;
        string streamType = "";
        Guid streamId = default;
        int streamVersion = default;
        Guid? tenantId = default;
        DateTimeOffset createdAt = default;
        Guid? createdByUserId = default;
        object? payload = default;

        await foreach (var column in insertMessage.NewRow.ConfigureAwait(false))
        {
          // TODO add error handling

          switch (column.GetFieldName())
          {
            case nameof(MessageEntity.Id):
              {
                eventId = await column.Get<Guid>(stoppingToken).ConfigureAwait(false);
                break;
              }
            case nameof(MessageEntity.GlobalIndex):
              {
                globalIndex = await column.Get<long>(stoppingToken).ConfigureAwait(false);
                break;
              }
            case nameof(MessageEntity.StreamType):
              {
                streamType = await column.GetTextReader().ReadToEndAsync(stoppingToken).ConfigureAwait(false);
                break;
              }
            case nameof(MessageEntity.StreamId):
              {
                streamId = await column.Get<Guid>(stoppingToken).ConfigureAwait(false);
                break;
              }
            case nameof(MessageEntity.StreamVersion):
              {
                streamVersion = await column.Get<int>(stoppingToken).ConfigureAwait(false);
                break;
              }
            case nameof(MessageEntity.TenantId):
              {
                tenantId = column.IsDBNull ? null : await column.Get<Guid>(stoppingToken).ConfigureAwait(false);
                break;
              }
            case nameof(MessageEntity.CreatedAt):
              {
                createdAt = await column.Get<DateTimeOffset>(stoppingToken).ConfigureAwait(false);
                break;
              }
            case nameof(MessageEntity.CreatedByUserId):
              {
                createdByUserId = column.IsDBNull ? null : await column.Get<Guid>(stoppingToken).ConfigureAwait(false);
                break;
              }
            case nameof(MessageEntity.PayloadJson):
              {
                var payloadType = DomainExtensions.GetTypeFromName(streamType);
                var payloadJson = await column.GetTextReader().ReadToEndAsync(stoppingToken).ConfigureAwait(false);
                payload = JsonSerializer.Deserialize(payloadJson, payloadType!, _options.JsonOptions);
                break;
              }
          }
        }

        if (streamVersion > 0)
        {
          var metadata = new DomainEventMetadata
          {
            EventId = eventId,
            StreamType = streamType,
            StreamId = streamId,
            StreamVersion = streamVersion,
            TenantId = tenantId,
            CreatedAt = createdAt,
            CreatedByUserId = createdByUserId,
          };

          var messageType = typeof(DomainEvent<>).MakeGenericType(payload!.GetType());
          var domainEvent = Activator.CreateInstance(messageType);
          messageType.GetProperty(nameof(DomainEvent<>.Metadata))?.SetValue(domainEvent, metadata);
          messageType.GetProperty(nameof(DomainEvent<>.Payload))?.SetValue(domainEvent, payload);

          _logger.LogInformation("Received domain event {DomainEvent}", domainEvent);
          // TODO push to Orleans stream
        }
        else if (streamVersion == 0)
        {
          _logger.LogInformation("Received public message {Message}", payload);
          // TODO push to Orleans stream
        }
      }

      conn.SetReplicationStatus(message.WalEnd);
    }
  }
}
