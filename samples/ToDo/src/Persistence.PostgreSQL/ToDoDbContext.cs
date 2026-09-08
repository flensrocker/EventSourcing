using Flensrocker.ToDo.Abstractions;
using Flensrocker.ToDo.Persistence.PostgreSQL.EntityTypeConfigurations;
using Microsoft.EntityFrameworkCore;

namespace Flensrocker.ToDo.Persistence.PostgreSQL;

public class ToDoDbContext : DbContext
{
  public DbSet<ToDoView.ListItem> ListItem
  {
    get => Set<ToDoView.ListItem>();
  }

  public ToDoDbContext(DbContextOptions<ToDoDbContext> options)
    : base(options) { }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.HasDefaultSchema(Constants.SchemaName);

    modelBuilder.ApplyConfiguration(new ToDoViewListItemConfiguration());
  }
}
