using Flensrocker.EventSourcing.Abstractions;
using Flensrocker.EventSourcing.Persistence.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Flensrocker.EventSourcing.Persistence.PostgreSQL;

internal class PostgreSQLPersistenceDbMigration(
  IDbContextFactory<EventSourcingDbContext> dbContextFactory,
  ILogger<PostgreSQLPersistenceDbMigration> logger
) : IDbMigration
{
  private readonly IDbContextFactory<EventSourcingDbContext> _dbContextFactory = dbContextFactory;
  private readonly ILogger<PostgreSQLPersistenceDbMigration> _logger = logger;

  public async Task MigrateAsync()
  {
    _logger.LogInformation("Migration starting on DbContext {DbContext}", typeof(EventSourcingDbContext));

    using (var migrationDbContext = _dbContextFactory.CreateDbContext())
      await migrationDbContext.Database.MigrateAsync().ConfigureAwait(false);

    using (var slotDbContext = _dbContextFactory.CreateDbContext())
    {
      var createSlot = $"""
        DO $$
        BEGIN
          SELECT * FROM pg_create_logical_replication_slot('{PostgreSQLPersistenceConstants.PublicationSlotName}', 'pgoutput');
          EXCEPTION WHEN OTHERS THEN NULL;
        END$$;
        """;

      await slotDbContext.Database.ExecuteSqlRawAsync(createSlot).ConfigureAwait(false);
    }

    _logger.LogInformation("Migration finished on DbContext {DbContext}", typeof(EventSourcingDbContext));
  }
}
