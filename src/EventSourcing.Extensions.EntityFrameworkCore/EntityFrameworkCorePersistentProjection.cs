using Flensrocker.EventSourcing.Abstractions;
using Flensrocker.EventSourcing.Abstractions.PersistentProjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Flensrocker.EventSourcing.Extensions.EntityFrameworkCore;

public abstract partial class EntityFrameworkCorePersistentProjection<TDbContext, TEntity, TEventBase>(
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
    LogRebuildStarting(_logger, typeof(TDbContext), typeof(TEntity), typeof(TEventBase));

    await _dbContextFactory
      .RebuildProjectionAsync<TDbContext, TEntity, TEventBase>(_eventStore, _logger, _projectionOptions, default)
      .ConfigureAwait(false);

    LogRebuildFinished(_logger, typeof(TDbContext), typeof(TEntity), typeof(TEventBase));
  }

  [LoggerMessage(
    Level = LogLevel.Information,
    Message = "Rebuild starting on DbContext {DbContext}, entity {Entity} on events {EventBase}"
  )]
  internal static partial void LogRebuildStarting(ILogger logger, Type dbContext, Type entity, Type eventBase);

  [LoggerMessage(
    Level = LogLevel.Information,
    Message = "Rebuild finished on DbContext {DbContext}, entity {Entity} on events {EventBase}"
  )]
  internal static partial void LogRebuildFinished(ILogger logger, Type dbContext, Type entity, Type eventBase);
}
