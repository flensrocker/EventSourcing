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
    public IServiceCollection AddEntityFrameworkCoreDbMigration<TDbContext>()
      where TDbContext : DbContext => services.AddDbMigration<EntityFrameworkCoreDbMigration<TDbContext>>();

    public IServiceCollection AddEntityFrameworkCorePersistentProjection<TProjection, TDbContext, TEntity, TEventBase>(
      string projectionTypeName,
      int currentVersion
    )
      where TProjection : EntityFrameworkCorePersistentProjection<TDbContext, TEntity, TEventBase>
      where TDbContext : DbContext
      where TEntity : class, IDomainEventProjectable, IWhereKey<TEntity>, new()
      where TEventBase : class => services.AddPersistentProjection<TProjection>(projectionTypeName, currentVersion);

    public IServiceCollection AddEntityFrameworkCoreStrongConsistentProjection<TEventBase, TDbContext, TProjection>()
      where TEventBase : class
      where TDbContext : DbContext
      where TProjection : class, IDbStrongConsistentProjection<TEventBase, TDbContext>
    {
      services.AddScoped<TProjection>();
      services.AddScoped<IDbStrongConsistentProjection<TEventBase, TDbContext>>(sp => sp.GetRequiredService<TProjection>());

      return services;
    }
  }

  public static async Task<TEntity?> UpdateProjectionAsync<TDbContext, TEntity, TEventBase>(
    this TDbContext dbContext,
    IDomainEventStorage<TEventBase> eventStore,
    IEnumerable<DomainEvent> domainEvents,
    TEntity? projection,
    string projectionTypeName,
    ILogger logger
  )
    where TDbContext : DbContext
    where TEntity : class, IDomainEventProjectable, IWhereKey<TEntity>, new()
    where TEventBase : class
  {
    var firstEvent = domainEvents.FirstOrDefault();
    if (firstEvent is null)
      return projection;

    var lastEvent = domainEvents.Last();

    var entityId = firstEvent.Metadata.StreamId;
    var firstExpectedVersion = firstEvent.Metadata.StreamVersion - 1;
    var lastExpectedVersion = lastEvent.Metadata.StreamVersion - 1;

    try
    {
      projection ??= await dbContext
        .Set<TEntity>()
        .AsNoTracking()
        .Where(TEntity.GetWhereKey(firstEvent.Metadata))
        .FirstOrDefaultAsync()
        .ConfigureAwait(false);
      if (projection is IDomainEventVersionable hasVersion && hasVersion.Version > lastExpectedVersion)
        return projection;

      var entityState = EntityState.Modified;
      if (projection is null)
      {
        projection = new();
        entityState = EntityState.Added;
      }

      var entity = dbContext.Attach(projection);

      if (projection is IDomainEventVersionable withVersion && withVersion.Version < firstExpectedVersion)
      {
        logger.LogWarning(
          "Entity {ProjectionType}, entity-id {EntityId} is missing some events, expected version {ExpectedVersion}, got version {ActualVersion}",
          projectionTypeName,
          entityId,
          firstExpectedVersion,
          withVersion.Version
        );

        var fromVersion = Math.Max(1, withVersion.Version);
        var missingEvents = await eventStore
          .ReadEventsFromStorage(entityId, fromVersion, firstExpectedVersion + 1, default)
          .ConfigureAwait(false);
        if (missingEvents is null)
        {
          logger.LogError(
            "Failure in retrieving missing events from storage for type {ProjectionType}, entity-id {EntityId} and version range {FromVersion} to {ExpectedVersion}",
            projectionTypeName,
            entityId,
            fromVersion,
            firstExpectedVersion
          );
          return projection;
        }

        projection.ApplyEvents(missingEvents);
      }

      projection.ApplyEvents(domainEvents);

      entity.State = entityState;
      await dbContext.SaveChangesAsync().ConfigureAwait(false);
    }
    catch (Exception ex)
    {
      logger.LogError(
        ex,
        "Failure in applying updates to projection of type {ProjectionType}, entity-id {EntityId} and expected version {FirstExpectedVersion}-{LastExpectedVersion}",
        projectionTypeName,
        entityId,
        firstExpectedVersion,
        lastExpectedVersion
      );
    }

    return projection;
  }

  public static async Task RebuildProjectionAsync<TDbContext, TEntity, TEventBase>(
    this IDbContextFactory<TDbContext> dbContextFactory,
    IDomainEventStorage<TEventBase> eventStore,
    ILogger logger,
    PersistentProjectionOptions projectionOptions,
    CancellationToken cancellationToken
  )
    where TDbContext : DbContext
    where TEntity : class, IDomainEventProjectable, IWhereKey<TEntity>, new()
    where TEventBase : class
  {
    var batchSize = 1000;
    logger.LogInformation("Rebuilding projection {ProjectionType}", projectionOptions.TypeName);

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
            projection = await dbContext
              .Set<TEntity>()
              .Where(TEntity.GetWhereKey(domainEvent.Metadata))
              .FirstOrDefaultAsync(cancellationToken)
              .ConfigureAwait(false);
            if (projection is null)
            {
              logger.LogWarning(
                "Missing projection {ProjectionType}, entity-id {EntityId}, version {EntityVersion}",
                projectionOptions.TypeName,
                domainEvent.Metadata.StreamId,
                domainEvent.Metadata.StreamVersion
              );

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
      logger.LogInformation(
        "Rebuilt {BatchTotalCount} projections with {DomainEventTotalCount} events",
        batchTotalCount,
        domainEventTotalCount
      );
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
