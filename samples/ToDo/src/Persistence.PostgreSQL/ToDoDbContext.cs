using Flensrocker.EventSourcing.Persistence.PostgreSQL;
using Flensrocker.ToDo.Abstractions.Tasks;
using Flensrocker.ToDo.Persistence.PostgreSQL.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Flensrocker.ToDo.Persistence.PostgreSQL;

public class ToDoDbContext : DbContext
{
  public DbSet<TaskView.DailyStats> TaskDailyStats
  {
    get => Set<TaskView.DailyStats>();
  }
  public DbSet<TaskView.ListItem> TaskListItem
  {
    get => Set<TaskView.ListItem>();
  }

  public ToDoDbContext(DbContextOptions<ToDoDbContext> options)
    : base(options) { }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    // NOTE BEST PRACTICE
    // This is only needed if you implement strong consistent projections on your DbContext
    modelBuilder.ApplyPostgreSQLEventSourcingConfiguration(excludeFromMigrations: true);

    modelBuilder.HasDefaultSchema(ToDoPostgreSQLPersistenceConstants.SchemaName);

    modelBuilder.ApplyConfiguration(new TaskViewDailyStatsEntityConfiguration());
    modelBuilder.ApplyConfiguration(new TaskViewListItemEntityConfiguration());
  }
}
