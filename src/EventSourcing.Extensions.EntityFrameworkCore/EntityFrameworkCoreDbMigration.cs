using Flensrocker.EventSourcing.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Flensrocker.EventSourcing.Extensions.EntityFrameworkCore;

internal class EntityFrameworkCoreDbMigration<TDbContext>(TDbContext dbContext, ILogger<EntityFrameworkCoreDbMigration<TDbContext>> logger)
  : IDbMigration
  where TDbContext : DbContext
{
  private readonly TDbContext _dbContext = dbContext;
  private readonly ILogger _logger = logger;

  public async Task MigrateAsync()
  {
    _logger.LogInformation("Migration starting on DbContext {DbContext}", _dbContext.GetType());

    await _dbContext.Database.MigrateAsync().ConfigureAwait(false);

    _logger.LogInformation("Migration finished on DbContext {DbContext}", _dbContext.GetType());
  }
}
