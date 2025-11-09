using Flensrocker.ToDo.Abstractions.Views;
using Microsoft.EntityFrameworkCore;

namespace Flensrocker.ToDo.Persistence.PostgreSQL;

public class ToDoDbContext : DbContext
{
  public DbSet<ToDoListItemView> ListItem { get => Set<ToDoListItemView>(); }

  public ToDoDbContext(DbContextOptions<ToDoDbContext> options)
    : base(options)
  {
  }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.HasDefaultSchema(Constants.SchemaName);

    modelBuilder.ApplyConfiguration(new ToDoListItemViewConfiguration());
  }
}
