using Flensrocker.EventSourcing.Abstractions;
using Flensrocker.EventSourcing.Abstractions.PersistentProjection;
using Flensrocker.EventSourcing.Extensions.EntityFrameworkCore;
using Flensrocker.EventSourcing.Orleans.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans.Streams;
using Orleans.Streams.Core;

namespace Flensrocker.EventSourcing.Orleans.EntityFrameworkCore;

public abstract class EntityFrameworkCoreUpdateProjectionGrain<TDbContext, TEntity, TEventBase>
  : Grain,
    IStreamSubscriptionObserver,
    IAsyncObserver<DomainEvent>,
    IUpdateDomainEventProjectionGrain<TEntity>
  where TDbContext : DbContext
  where TEntity : class, IDomainEventProjectable, IWhereKey<TEntity>, new()
  where TEventBase : class
{
  private readonly OrleansEntityFrameworkCoreProjectionMetrics _metrics;
  private readonly ILogger _logger;
  private readonly PersistentProjectionOptions _projectionOptions;

  private TEntity? _projection;

  protected EntityFrameworkCoreUpdateProjectionGrain(string projectionTypeName)
  {
    _metrics = ServiceProvider.GetRequiredService<OrleansEntityFrameworkCoreProjectionMetrics>();
    _logger = ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(GetType());
    _projectionOptions = ServiceProvider.GetRequiredKeyedService<PersistentProjectionOptions>(projectionTypeName);
  }

  public async Task OnSubscribed(IStreamSubscriptionHandleFactory handleFactory)
  {
    var handle = handleFactory.Create<DomainEvent>();
    await handle.ResumeAsync(this);
  }

  public async Task OnNextAsync(DomainEvent domainEvent, StreamSequenceToken? token = null)
  {
    _metrics.ProjectionMessagesReceived();

    await UpdateProjection(domainEvent);

    _metrics.ProjectionMessagesSaved();
  }

  public Task OnErrorAsync(Exception ex)
  {
    _metrics.ProjectionStreamErrored();

    // TODO add error handling
    _logger.LogError(ex, "Error from Orleans Stream");

    return Task.CompletedTask;
  }

  public async Task UpdateProjection(DomainEvent domainEvent)
  {
    var expectedVersion = domainEvent.Metadata.StreamVersion - 1;
    if (_projection is IDomainEventVersionable versionable && versionable.Version > expectedVersion)
    {
      _metrics.ProjectionMessagesSkipped();
      return;
    }

    await using var scope = ServiceProvider.CreateAsyncScope();

    var strategyDbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();

    await strategyDbContext
      .Database.CreateExecutionStrategy()
      .ExecuteAsync(async () =>
      {
        var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();
        var eventStore = scope.ServiceProvider.GetRequiredService<IDomainEventStorage<TEventBase>>();

        _projection = await dbContext.UpdateProjectionAsync(eventStore, [domainEvent], _projection, _projectionOptions.TypeName, _logger);
      });
  }
}
