using System.Linq.Expressions;
using Flensrocker.Domain.Abstractions;
using Flensrocker.ToDo.Abstractions;
using Flensrocker.ToDo.Abstractions.Views;
using Flensrocker.ToDo.Orleans.Abstractions;
using Flensrocker.ToDo.Persistence.PostgreSQL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans.Streams;
using Orleans.Streams.Core;

namespace Flensrocker.ToDo.Orleans.Server;

[ImplicitStreamSubscription(ToDoNames.ToDoEventStream)]
public class ToDoPersistListItemGrain : PersistEntityFrameworkCoreGrain<ToDoPersistListItemGrain, ToDoDbContext, ToDoListItemView, ToDoEvent>
{
  public ToDoPersistListItemGrain()
    : base(entityId => e => e.ToDoId == entityId)
  {
  }
}

public abstract class PersistEntityFrameworkCoreGrain<TGrain, TDbContext, TEntity, TEventBase>
  : Grain, IGrainWithGuidKey, IStreamSubscriptionObserver, IAsyncObserver<DomainEvent<TEventBase>>
  where TGrain : PersistEntityFrameworkCoreGrain<TGrain, TDbContext, TEntity, TEventBase>
  where TDbContext : DbContext
  where TEntity : class, IDomainEventProjectable<TEventBase>, new()
  where TEventBase : class
{
  private readonly IDomainEventStorage<TEventBase> _eventStore;
  private readonly IDbContextFactory<TDbContext> _dbContextFactory;
  private readonly ILogger _logger;
  private readonly Func<Guid, Expression<Func<TEntity, bool>>> _whereId;

  private TEntity? _projection;

  protected PersistEntityFrameworkCoreGrain(Func<Guid, Expression<Func<TEntity, bool>>> whereId)
  {
    _whereId = whereId;

    _eventStore = ServiceProvider.GetRequiredService<IDomainEventStorage<TEventBase>>();
    _dbContextFactory = ServiceProvider.GetRequiredService<IDbContextFactory<TDbContext>>();
    _logger = ServiceProvider.GetRequiredService<ILogger<TGrain>>();
  }

  public async Task OnSubscribed(IStreamSubscriptionHandleFactory handleFactory)
  {
    var handle = handleFactory.Create<DomainEvent<TEventBase>>();
    await handle.ResumeAsync(this);
  }

  public async Task OnNextAsync(DomainEvent<TEventBase> domainEvent, StreamSequenceToken? token = null)
  {
    _projection = await _dbContextFactory.UpdateProjectionAsync(_eventStore, _whereId, domainEvent, _projection, _logger);
  }

  public Task OnErrorAsync(Exception ex)
  {
    // TODO add error handling
    return Task.CompletedTask;
  }
}

public static class EntityFrameworkCoreDomainExtensions
{
  public static async Task<TEntity?> UpdateProjectionAsync<TDbContext, TEntity, TEventBase>(
    this IDbContextFactory<TDbContext> dbContextFactory,
    IDomainEventStorage<TEventBase> eventStore,
    Func<Guid, Expression<Func<TEntity, bool>>> whereId,
    DomainEvent<TEventBase> domainEvent,
    TEntity? projection,
    ILogger logger)
    where TDbContext : DbContext
    where TEntity : class, IDomainEventProjectable<TEventBase>, new()
    where TEventBase : class
  {
    var entityTypeName = typeof(TEntity).GetTypeName();
    var entityId = domainEvent.Metadata.StreamId;
    var expectedVersion = domainEvent.Metadata.StreamVersion - 1;

    try
    {
      using var strategyDbContext = await dbContextFactory.CreateDbContextAsync();

      projection = await strategyDbContext.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
      {
        using var dbContext = await dbContextFactory.CreateDbContextAsync();

        projection ??= await dbContext.Set<TEntity>().AsNoTracking().Where(whereId(entityId)).FirstOrDefaultAsync();

        var addToDbContext = false;
        if (projection is null)
        {
          projection = new();
          addToDbContext = true;
        }
        else if (projection is IDomainEventVersionable hasVersion && hasVersion.Version > expectedVersion)
          return projection;

        var entity = dbContext.Attach(projection);

        if (projection is IDomainEventVersionable withVersion && withVersion.Version < expectedVersion)
        {
          logger.LogWarning("Entity {EntityType}, entity-id {EntityId} is missing some events, expected version {ExpectedVersion}, got version {ActualVersion}", entityTypeName, entityId, expectedVersion, withVersion.Version);

          var fromVersion = Math.Max(1, withVersion.Version);
          var itemEvents = await eventStore.ReadEventsFromStorage(entityId, fromVersion, expectedVersion + 1, default);
          if (itemEvents is null)
          {
            logger.LogError("Failure in retrieving missing events from storage for type {EntityType}, entity-id {EntityId} and version range {FromVersion} to {ExpectedVersion}", entityTypeName, entityId, fromVersion, expectedVersion);
            return projection;
          }

          foreach (var itemEvent in itemEvents)
            projection.Apply(itemEvent);
        }

        projection.Apply(domainEvent);

        if (addToDbContext)
          entity.State = EntityState.Added;

        await dbContext.SaveChangesAsync();

        return projection;
      });
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Failure in applying updates to projection of type {EntityType}, entity-id {EntityId} and expected version {ExpectedVersion}", entityTypeName, entityId, expectedVersion);
    }

    return projection;
  }
}
