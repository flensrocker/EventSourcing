using Flensrocker.EventSourcing.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Flensrocker.EventSourcing.Orleans.EntityFrameworkCore;

public static class OrleansEntityFrameworkCoreExtensions
{
  public static IServiceCollection AddOrleansEntityFrameworkCoreProjections(this IServiceCollection services)
  {
    services.TryAddSingleton<OrleansEntityFrameworkCoreProjectionMetrics>();

    return services;
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
      projection ??= await dbContext.Set<TEntity>().AsNoTracking().Where(TEntity.GetWhereId(entityId)).FirstOrDefaultAsync();

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
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Failure in applying updates to projection of type {EntityType}, entity-id {EntityId} and expected version {ExpectedVersion}", entityTypeName, entityId, expectedVersion);
    }

    return projection;
  }
}
