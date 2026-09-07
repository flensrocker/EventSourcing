using Microsoft.EntityFrameworkCore;

namespace Flensrocker.EventSourcing.Persistence.PostgreSQL;

internal class EventSourcingDbContext : DbContext
{
  public EventSourcingDbContext(DbContextOptions<EventSourcingDbContext> options)
    : base(options) { }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.HasDefaultSchema(Constants.SchemaName);

    modelBuilder.ApplyConfiguration(new MessageEntiyConfiguration(modelBuilder));
  }
}
