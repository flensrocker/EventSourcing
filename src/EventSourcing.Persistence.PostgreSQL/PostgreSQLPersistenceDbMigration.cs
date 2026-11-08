using Flensrocker.EventSourcing.Extensions.EntityFrameworkCore;
using Flensrocker.EventSourcing.Persistence.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Flensrocker.EventSourcing.Persistence.PostgreSQL;

internal class PostgreSQLPersistenceDbMigration(
  IDbContextFactory<EventSourcingDbContext> dbContextFactory,
  ILogger<EntityFrameworkCoreDbMigration<EventSourcingDbContext>> logger
) : EntityFrameworkCoreDbMigration<EventSourcingDbContext>(dbContextFactory, logger)
{
  public override async Task MigrateAsync()
  {
    await base.MigrateAsync().ConfigureAwait(false);

    using var slotDbContext = _dbContextFactory.CreateDbContext();
    var createSlot = $"""
      DO $$
      BEGIN
        SELECT * FROM pg_create_logical_replication_slot('{PostgreSQLPersistenceConstants.PublicationSlotName}', 'pgoutput');
        EXCEPTION WHEN OTHERS THEN NULL;
      END$$;
      """;

    await slotDbContext.Database.ExecuteSqlRawAsync(createSlot).ConfigureAwait(false);
  }
}
