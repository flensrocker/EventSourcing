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
    await using var scope = ServiceProvider.CreateAsyncScope();

    var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();
    var eventStore = scope.ServiceProvider.GetRequiredService<IDomainEventStorage<TEventBase>>();

    _projection = await dbContext.UpdateProjectionAsync(eventStore, domainEvent, _projection, _logger);
  }

  public Task OnErrorAsync(Exception ex)
  {
    // TODO add error handling
    return Task.CompletedTask;
  }
}
