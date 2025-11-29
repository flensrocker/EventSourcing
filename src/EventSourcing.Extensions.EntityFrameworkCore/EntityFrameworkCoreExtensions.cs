using Flensrocker.EventSourcing.Abstractions;
using Flensrocker.EventSourcing.Abstractions.PersistentProjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Flensrocker.EventSourcing.Extensions.EntityFrameworkCore;

public static class EntityFrameworkCoreExtensions
{
  extension(IServiceCollection services)
  {
    public IServiceCollection AddEntityFrameworkCoreDbMigration<TDbContext>() where TDbContext : DbContext
      => services.AddDbMigration<EntityFrameworkCoreDbMigration<TDbContext>>();

    public IServiceCollection AddEntityFrameworkCorePersistentProjection<TProjection, TDbContext, TEntity, TEventBase>(int currentVersion)
      where TProjection : EntityFrameworkCorePersistentProjection<TDbContext, TEntity, TEventBase>
      where TDbContext : DbContext
      where TEntity : class, IDomainEventProjectable, IWhereId<TEntity>, new()
      where TEventBase : class
      => services.AddPersistentProjection<TProjection>(currentVersion);
  }

  public static async Task<TEntity?> UpdateProjectionAsync<TDbContext, TEntity, TEventBase>(
    this TDbContext dbContext,
    IDomainEventStorage<TEventBase> eventStore,
    DomainEvent domainEvent,
    TEntity? projection,
    ILogger logger)
    where TDbContext : DbContext
    where TEntity : class, IDomainEventProjectable, IWhereId<TEntity>, new()
    where TEventBase : class
  {
    var entityTypeName = typeof(TEntity).GetTypeName();
    var entityId = domainEvent.Metadata.StreamId;
    var expectedVersion = domainEvent.Metadata.StreamVersion - 1;

    try
    {
      projection ??= await dbContext.Set<TEntity>().AsNoTracking().Where(TEntity.GetWhereId(entityId)).FirstOrDefaultAsync().ConfigureAwait(false);
      if (projection is IDomainEventVersionable hasVersion && hasVersion.Version > expectedVersion)
        return projection;

      var addToDbContext = false;
      if (projection is null)
      {
        projection = new();
        addToDbContext = true;
      }

      var entity = dbContext.Attach(projection);

      if (projection is IDomainEventVersionable withVersion && withVersion.Version < expectedVersion)
      {
        logger.LogWarning("Entity {EntityType}, entity-id {EntityId} is missing some events, expected version {ExpectedVersion}, got version {ActualVersion}", entityTypeName, entityId, expectedVersion, withVersion.Version);

        var fromVersion = Math.Max(1, withVersion.Version);
        var itemEvents = await eventStore.ReadEventsFromStorage(entityId, fromVersion, expectedVersion + 1, default).ConfigureAwait(false);
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

      await dbContext.SaveChangesAsync().ConfigureAwait(false);
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Failure in applying updates to projection of type {EntityType}, entity-id {EntityId} and expected version {ExpectedVersion}", entityTypeName, entityId, expectedVersion);
    }

    return projection;
  }

  public static async Task RebuildProjectionAsync<TDbContext, TEntity, TEventBase>(
    this IDbContextFactory<TDbContext> dbContextFactory,
    IDomainEventStorage<TEventBase> eventStore,
    ILogger logger,
    CancellationToken cancellationToken)
    where TDbContext : DbContext
    where TEntity : class, IDomainEventProjectable, IWhereId<TEntity>, new()
    where TEventBase : class
  {
    var batchSize = 1000;
    var entityTypeName = typeof(TEntity).GetTypeName();
    logger.LogInformation("Rebuilding projection {EntityType}", entityTypeName);

    TDbContext? dbContext = null;
    try
    {
      dbContext = await dbContextFactory.CreateDbContextAsync().ConfigureAwait(false);

      await dbContext.Set<TEntity>().ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);

      var batchCount = 0;
      var batchTotalCount = 0;
      var domainEventCount = 0;
      var domainEventTotalCount = 0;

      await foreach (var domainEvent in eventStore.ReadAllEventsFromStorage(cancellationToken).ConfigureAwait(false))
      {
        TEntity? projection;

        if (domainEvent.Metadata.StreamVersion == 1)
        {
          projection = new();
          var entity = dbContext.Attach(projection);
          entity.State = EntityState.Added;

          batchCount++;
        }
        else
        {
          var entity = dbContext.Set<TEntity>().Local.FindEntry(domainEvent.Metadata.StreamId);
          if (entity is null)
          {
            projection = await dbContext.Set<TEntity>().Where(TEntity.GetWhereId(domainEvent.Metadata.StreamId)).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
            if (projection is null)
            {
              logger.LogWarning("Missing projection {EntityType}, entity-id {EntityId}, version {EntityVersion}", entityTypeName, domainEvent.Metadata.StreamId, domainEvent.Metadata.StreamVersion);

              projection = new();
              entity = dbContext.Attach(projection);
              entity.State = EntityState.Added;

              batchCount++;
            }
          }
          else
            projection = entity.Entity;
        }

        projection.Apply(domainEvent);
        domainEventCount++;

        if (batchCount >= batchSize)
        {
          logger.LogInformation("Batch save after {BatchCount} projections with {DomainEventCount} events", batchCount, domainEventCount);
          await dbContext.SaveChangesAsync().ConfigureAwait(false);

          await dbContext.DisposeAsync().ConfigureAwait(false);
          dbContext = await dbContextFactory.CreateDbContextAsync().ConfigureAwait(false);

          batchTotalCount += batchCount;
          domainEventTotalCount += domainEventCount;
          batchCount = 0;
          domainEventCount = 0;
        }
      }

      logger.LogInformation("Final save after {BatchCount} projections with {DomainEventCount} events", batchCount, domainEventCount);
      await dbContext.SaveChangesAsync().ConfigureAwait(false);

      batchTotalCount += batchCount;
      domainEventTotalCount += domainEventCount;
      logger.LogInformation("Rebuilt {BatchTotalCount} projections with {DomainEventTotalCount} events", batchTotalCount, domainEventTotalCount);
    }
    finally
    {
      if (dbContext is not null)
      {
        await dbContext.DisposeAsync().ConfigureAwait(false);
        dbContext = null;
      }
    }
  }
}
