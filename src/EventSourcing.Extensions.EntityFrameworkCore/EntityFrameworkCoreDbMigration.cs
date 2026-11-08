using Flensrocker.EventSourcing.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Flensrocker.EventSourcing.Extensions.EntityFrameworkCore;

public partial class EntityFrameworkCoreDbMigration<TDbContext>(
  IDbContextFactory<TDbContext> dbContextFactory,
  ILogger<EntityFrameworkCoreDbMigration<TDbContext>> logger
) : IDbMigration
  where TDbContext : DbContext
{
  protected readonly IDbContextFactory<TDbContext> _dbContextFactory = dbContextFactory;
  protected readonly ILogger _logger = logger;

  public virtual async Task MigrateAsync()
  {
    LogMigrationStarting(_logger, typeof(TDbContext));

    using (var migrationDbContext = _dbContextFactory.CreateDbContext())
      await migrationDbContext.Database.MigrateAsync().ConfigureAwait(false);

    LogMigrationFinished(_logger, typeof(TDbContext));
  }

  [LoggerMessage(Level = LogLevel.Information, Message = "Migration starting on DbContext {DbContext}")]
  internal static partial void LogMigrationStarting(ILogger logger, Type dbContext);

  [LoggerMessage(Level = LogLevel.Information, Message = "Migration finished on DbContext {DbContext}")]
  internal static partial void LogMigrationFinished(ILogger logger, Type dbContext);
}
