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
  private readonly IDomainEventStorage<TEventBase> _eventStore;
  private readonly IDbContextFactory<TDbContext> _dbContextFactory;
  private readonly ILogger _logger;

  private TEntity? _projection;

  protected EntityFrameworkCoreProjectionGrain()
  {
    _eventStore = ServiceProvider.GetRequiredService<IDomainEventStorage<TEventBase>>();
    _dbContextFactory = ServiceProvider.GetRequiredService<IDbContextFactory<TDbContext>>();
    _logger = ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(GetType());
  }

  public async Task OnSubscribed(IStreamSubscriptionHandleFactory handleFactory)
  {
    var handle = handleFactory.Create<DomainEvent>();
    await handle.ResumeAsync(this);
  }

  public async Task OnNextAsync(DomainEvent domainEvent, StreamSequenceToken? token = null)
  {
    _projection = await _dbContextFactory.UpdateProjectionAsync(_eventStore, domainEvent, _projection, _logger);
  }

  public Task OnErrorAsync(Exception ex)
  {
    // TODO add error handling
    return Task.CompletedTask;
  }
}
