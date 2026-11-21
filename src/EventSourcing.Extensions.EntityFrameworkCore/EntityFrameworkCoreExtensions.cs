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

  extension<TDbContext>(TDbContext dbContext)
    where TDbContext : DbContext
  {
    public async Task<TEntity?> UpdateProjectionAsync<TEntity, TEventBase>(
      IDomainEventStorage<TEventBase> eventStore,
      IReadOnlyList<DomainEvent> domainEvents,
      TEntity? projection,
      string projectionTypeName,
      ILogger logger
    )
      where TEntity : class, IDomainEventProjectable, IWhereKey<TEntity>, new()
      where TEventBase : class
    {
      if (domainEvents.Count == 0)
        return projection;

      // TODO delete/restart projection if sequence contains "deletion event"
      var firstEvent = domainEvents[0];
      var lastEvent = domainEvents[domainEvents.Count - 1];

      var entityId = firstEvent.Metadata.StreamId;
      var firstExpectedVersion = firstEvent.Metadata.StreamVersion - 1;
      var lastExpectedVersion = lastEvent.Metadata.StreamVersion - 1;

      projection ??= await dbContext
        .Set<TEntity>()
        .AsNoTracking()
        .Where(TEntity.GetWhereKey(firstEvent))
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

      var entityEntry = dbContext.Attach(projection);

      if (projection is IDomainEventVersionable withVersion && withVersion.Version < firstExpectedVersion)
      {
        logger.LogWarning(
          "Entity {ProjectionType}, entity-id {EntityId} is missing some events, expected version {ExpectedVersion}, got version {ActualVersion}",
          projectionTypeName,
          entityId,
          firstExpectedVersion,
          withVersion.Version
        );

        var fromVersion = Math.Max(1, withVersion.Version + 1);
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

      entityEntry.State = entityState;
      await dbContext.SaveChangesAsync().ConfigureAwait(false);

      return projection;
    }
  }

  extension<TDbContext>(IDbContextFactory<TDbContext> dbContextFactory)
    where TDbContext : DbContext
  {
    public async Task RebuildProjectionAsync<TEntity, TEventBase>(
      IDomainEventStorage<TEventBase> eventStore,
      ILogger logger,
      PersistentProjectionOptions projectionOptions,
      CancellationToken cancellationToken
    )
      where TEntity : class, IDomainEventProjectable, IWhereKey<TEntity>, new()
      where TEventBase : class
    {
      var batchSize = 1000;
      logger.LogInformation("Rebuilding projection {ProjectionType}", projectionOptions.TypeName);

      TDbContext? dbContext = null;
      try
      {
        dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        // TODO maybe batch on large tables (there's no EF Core "Truncate")
        await dbContext.Set<TEntity>().ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);

        var batchCount = 0;
        var batchTotalCount = 0;
        var domainEventCount = 0;
        var domainEventTotalCount = 0;

        await foreach (var domainEvent in eventStore.ReadAllEventsFromStorage(cancellationToken).ConfigureAwait(false))
        {
          // TODO if "deletion event" => remove projection, end batch to SaveChanges and clear ChangeTracker?
          var projection = dbContext.Set<TEntity>().Local.Where(TEntity.GetWhereKey(domainEvent).Compile().Invoke).FirstOrDefault();
          if (projection is null)
          {
            projection = await dbContext
              .Set<TEntity>()
              .Where(TEntity.GetWhereKey(domainEvent))
              .FirstOrDefaultAsync(cancellationToken)
              .ConfigureAwait(false);
            if (projection is null)
            {
              // TODO review: log error only if single-stream projection and not creation event
              if (domainEvent.Metadata.StreamVersion > 1)
              {
                logger.LogWarning(
                  "Missing projection {ProjectionType}, entity-id {EntityId}, version {EntityVersion}",
                  projectionOptions.TypeName,
                  domainEvent.Metadata.StreamId,
                  domainEvent.Metadata.StreamVersion
                );
              }

              projection = new();
              var entity = dbContext.Attach(projection);
              entity.State = EntityState.Added;

              batchCount++;
            }
          }

          projection?.Apply(domainEvent);
          domainEventCount++;

          if (batchCount >= batchSize)
          {
            logger.LogInformation("Batch save after {BatchCount} projections with {DomainEventCount} events", batchCount, domainEventCount);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            await dbContext.DisposeAsync().ConfigureAwait(false);
            dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

            batchTotalCount += batchCount;
            domainEventTotalCount += domainEventCount;
            batchCount = 0;
            domainEventCount = 0;
          }
        }

        logger.LogInformation("Final save after {BatchCount} projections with {DomainEventCount} events", batchCount, domainEventCount);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

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
          await dbContext.DisposeAsync().ConfigureAwait(false);
      }
    }
  }
}
