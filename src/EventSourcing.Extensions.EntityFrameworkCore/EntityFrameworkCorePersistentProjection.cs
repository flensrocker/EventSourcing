using Flensrocker.EventSourcing.Abstractions;
using Flensrocker.EventSourcing.Abstractions.PersistentProjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Flensrocker.EventSourcing.Extensions.EntityFrameworkCore;

public abstract class EntityFrameworkCorePersistentProjection<TDbContext, TEntity, TEventBase>(
  IDbContextFactory<TDbContext> dbContextFactory,
  IDomainEventStorage<TEventBase> eventStore,
  ILogger logger,
  PersistentProjectionOptions projectionOptions
) : IPersistentProjection
  where TDbContext : DbContext
  where TEntity : class, IDomainEventProjectable, IWhereKey<TEntity>, new()
  where TEventBase : class
{
  private readonly IDbContextFactory<TDbContext> _dbContextFactory = dbContextFactory;
  private readonly IDomainEventStorage<TEventBase> _eventStore = eventStore;
  private readonly ILogger _logger = logger;
  private readonly PersistentProjectionOptions _projectionOptions = projectionOptions;

  public async Task RebuildAsync()
  {
    _logger.LogInformation(
      "Rebuild starting on DbContext {DbContext}, entity {Entity} on events {EventBase}",
      typeof(TDbContext),
      typeof(TEntity),
      typeof(TEventBase)
    );

    await _dbContextFactory
      .RebuildProjectionAsync<TDbContext, TEntity, TEventBase>(_eventStore, _logger, _projectionOptions, default)
      .ConfigureAwait(false);

    _logger.LogInformation(
      "Rebuild finished on DbContext {DbContext}, entity {Entity} on events {EventBase}",
      typeof(TDbContext),
      typeof(TEntity),
      typeof(TEventBase)
    );
  }
}
