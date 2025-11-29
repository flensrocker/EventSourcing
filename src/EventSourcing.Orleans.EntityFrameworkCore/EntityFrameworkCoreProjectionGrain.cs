using Flensrocker.EventSourcing.Abstractions;
using Flensrocker.EventSourcing.Extensions.EntityFrameworkCore;
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
  private readonly OrleansEntityFrameworkCoreProjectionMetrics _metrics;
  private readonly ILogger _logger;

  private TEntity? _projection;

  protected EntityFrameworkCoreProjectionGrain()
  {
    _metrics = ServiceProvider.GetRequiredService<OrleansEntityFrameworkCoreProjectionMetrics>();
    _logger = ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(GetType());
  }

  public async Task OnSubscribed(IStreamSubscriptionHandleFactory handleFactory)
  {
    var handle = handleFactory.Create<DomainEvent>();
    await handle.ResumeAsync(this);
  }

  public async Task OnNextAsync(DomainEvent domainEvent, StreamSequenceToken? token = null)
  {
    _metrics.ProjectionMessagesReceived();

    var expectedVersion = domainEvent.Metadata.StreamVersion - 1;
    if (_projection is IDomainEventVersionable hasVersion && hasVersion.Version > expectedVersion)
    {
      _metrics.ProjectionMessagesSkipped();
      return;
    }

    await using var scope = ServiceProvider.CreateAsyncScope();

    var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();
    var eventStore = scope.ServiceProvider.GetRequiredService<IDomainEventStorage<TEventBase>>();

    _projection = await dbContext.UpdateProjectionAsync(eventStore, domainEvent, _projection, _logger);

    _metrics.ProjectionMessagesSaved();
  }

  public Task OnErrorAsync(Exception ex)
  {
    _metrics.ProjectionStreamErrored();

    // TODO add error handling
    _logger.LogError(ex, "Error from Orleans Stream");

    return Task.CompletedTask;
  }
}
