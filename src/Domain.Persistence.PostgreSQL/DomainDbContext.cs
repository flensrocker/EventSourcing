using Microsoft.EntityFrameworkCore;

namespace Flensrocker.Domain.Persistence.PostgreSQL;

internal class DomainDbContext : DbContext
{
  public DbSet<MessageEntity> Message { get => Set<MessageEntity>(); }

  public DomainDbContext(DbContextOptions<DomainDbContext> options)
    : base(options)
  {
  }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.HasDefaultSchema(Constants.SchemaName);

    modelBuilder.ApplyConfiguration(new MessageEntiyConfiguration(modelBuilder));
  }
}
