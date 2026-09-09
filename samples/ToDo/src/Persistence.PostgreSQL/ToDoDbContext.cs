using Flensrocker.ToDo.Abstractions.Tasks;
using Flensrocker.ToDo.Persistence.PostgreSQL.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Flensrocker.ToDo.Persistence.PostgreSQL;

public class ToDoDbContext : DbContext
{
  public DbSet<TaskView.ListItem> TaskListItem
  {
    get => Set<TaskView.ListItem>();
  }

  public ToDoDbContext(DbContextOptions<ToDoDbContext> options)
    : base(options) { }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.HasDefaultSchema(ToDoPostgreSQLPersistenceConstants.SchemaName);

    modelBuilder.ApplyConfiguration(new TaskViewListItemEntityConfiguration());
  }
}
