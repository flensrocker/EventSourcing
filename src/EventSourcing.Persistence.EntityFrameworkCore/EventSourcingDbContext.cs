using Microsoft.EntityFrameworkCore;

namespace Flensrocker.EventSourcing.Persistence.EntityFrameworkCore;

public class EventSourcingDbContextOptions
{
  public required Action<ModelBuilder> CreateModel { get; init; }
}

public class EventSourcingDbContext : DbContext
{
  private readonly EventSourcingDbContextOptions _options;

  public EventSourcingDbContext(EventSourcingDbContextOptions options, DbContextOptions<EventSourcingDbContext> dbOptions)
    : base(dbOptions)
  {
    _options = options;
  }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.HasDefaultSchema(EntityFrameworkCoreConstants.SchemaName);

    _options.CreateModel(modelBuilder);
  }
}
