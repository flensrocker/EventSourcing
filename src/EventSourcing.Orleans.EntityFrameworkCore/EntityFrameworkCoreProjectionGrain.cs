using System.Diagnostics.Metrics;
using Flensrocker.EventSourcing.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans.Streams;
using Orleans.Streams.Core;

namespace Flensrocker.EventSourcing.Orleans.EntityFrameworkCore;

public abstract class EntityFrameworkCoreProjectionGrain<TDbContext, TEntity, TEventBase>
  : Grain, IGrainWithGuidKey, IStreamSubscriptionObserver, IAsyncObserver<DomainEvent>
  where TDbContext : DbContext
  where TEntity : class, IDomainEventProjectable, IWhereId<TEntity>, new()
  where TEventBase : class
{
  private static Meter _meter = new Meter("Flensrocker.EventSourcing", "1.0.0");
  private static Counter<long> _projectionMessagesReceived = _meter.CreateCounter<long>("projection-messages-received", "Messages", "The number of messages received from Orleans");
  private static Counter<long> _projectionMessagesSaved = _meter.CreateCounter<long>("projection-messages-saved", "Messages", "The number of messages saved to PostgreSQL");
  private static Counter<long> _projectionStreamErrored = _meter.CreateCounter<long>("projection-stream-errored", "Errors", "The number of errors from the Orleans Stream");

  private readonly ILogger _logger;

  private TEntity? _projection;

  protected EntityFrameworkCoreProjectionGrain()
  {
    _logger = ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(GetType());
  }

  public async Task OnSubscribed(IStreamSubscriptionHandleFactory handleFactory)
  {
    var handle = handleFactory.Create<DomainEvent>();
    await handle.ResumeAsync(this);
  }

  public async Task OnNextAsync(DomainEvent domainEvent, StreamSequenceToken? token = null)
  {
    _projectionMessagesReceived.Add(1L);

    await using var scope = ServiceProvider.CreateAsyncScope();

    var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();
    var eventStore = scope.ServiceProvider.GetRequiredService<IDomainEventStorage<TEventBase>>();

    _projection = await dbContext.UpdateProjectionAsync(eventStore, domainEvent, _projection, _logger);

    _projectionMessagesSaved.Add(1L);
  }

  public Task OnErrorAsync(Exception ex)
  {
    _projectionStreamErrored.Add(1L);

    // TODO add error handling
    _logger.LogError(ex, "Error from Orleans Stream");

    return Task.CompletedTask;
  }
}
