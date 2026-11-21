using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json.Serialization;
using Flensrocker.EventSourcing.Abstractions;
using Flensrocker.EventSourcing.Abstractions.PersistentProjection;
using Flensrocker.EventSourcing.Extensions.EntityFrameworkCore;
using Flensrocker.EventSourcing.Persistence.EntityFrameworkCore;
using Flensrocker.EventSourcing.Persistence.PostgreSQL;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Flensrocker.EventSourcing.UnitTests.Extensions.EntityFrameworkCore;

public static class FooConstants
{
  public const string EventBase = "FooEvent";
  public const string CreatedEvent = $"{EventBase}_Created";
  public const string UpdatedEvent = $"{EventBase}_Updated";
  public const string DeletedEvent = $"{EventBase}_Deleted";

  public const string ViewBase = "FooView";
  public const string VersionTrackerView = $"{ViewBase}_VersionTracker";

  public const string DbSchema = "Foo";
  public const string VersionTrackerTable = "VersionTracker";
}

[GenerateSerializer]
[Alias(FooConstants.EventBase)]
[JsonDerivedType(typeof(Created), FooConstants.CreatedEvent)]
[JsonDerivedType(typeof(Updated), FooConstants.UpdatedEvent)]
[JsonDerivedType(typeof(Deleted), FooConstants.DeletedEvent)]
public abstract record FooEvent
{
  private FooEvent() { }

  [GenerateSerializer]
  [Alias(FooConstants.CreatedEvent)]
  public sealed record Created() : FooEvent { }

  [GenerateSerializer]
  [Alias(FooConstants.UpdatedEvent)]
  public sealed record Updated() : FooEvent { }

  [GenerateSerializer]
  [Alias(FooConstants.DeletedEvent)]
  public sealed record Deleted() : FooEvent { }
}

public abstract record FooView
{
  private FooView() { }

  [GenerateSerializer]
  [Alias(FooConstants.VersionTrackerView)]
  public sealed record VersionTracker : FooView, IDomainEventProjectable, IDomainEventVersionable, IWhereKey<VersionTracker>
  {
    public const int ProjectionVersion = 2;

    [Id(0)]
    [JsonInclude]
    public Guid FooId { get; internal set; }

    [Id(1)]
    [JsonInclude]
    public int Version { get; internal set; }

    [Id(2)]
    [JsonInclude]
    public Guid? TenantId { get; internal set; }

    [Id(3)]
    [JsonInclude]
    public string AppliedVersions { get; internal set; } = "";

    static object IWhereKey<VersionTracker>.GetKey(DomainEvent domainEvent) => domainEvent.Metadata.StreamId;

    static Expression<Func<VersionTracker, bool>> IWhereKey<VersionTracker>.GetWhereKey(DomainEvent domainEvent) =>
      item => item.FooId == domainEvent.Metadata.StreamId;

    void IDomainEventProjectable.Apply(DomainEvent @event)
    {
      FooId = @event.Metadata.StreamId;
      Version = @event.Metadata.StreamVersion;
      TenantId = @event.Metadata.TenantId;
      AppliedVersions = string.IsNullOrWhiteSpace(AppliedVersions) ? $"{Version}" : string.Join(",", AppliedVersions, $"{Version}");
    }
  }
}

public class FooPostgreSQLPersistenceOptions
{
  public required string ConnectionName { get; init; }
}

[JsonSerializable(typeof(FooEvent))]
[JsonSerializable(typeof(FooView.VersionTracker))]
[JsonSourceGenerationOptions(AllowOutOfOrderMetadataProperties = true)]
public partial class FooJsonSerializerContext : JsonSerializerContext { }

public class FooViewVersionTrackerEntityConfiguration : IEntityTypeConfiguration<FooView.VersionTracker>
{
  public void Configure(EntityTypeBuilder<FooView.VersionTracker> builder)
  {
    builder.ToTable(FooConstants.VersionTrackerTable, FooConstants.DbSchema);

    builder.HasKey(e => e.FooId);

    builder.Property(e => e.Version);
    builder.Property(e => e.TenantId);
    builder.Property(e => e.AppliedVersions);
  }
}

public class FooDbContext : DbContext
{
  public DbSet<FooView.VersionTracker> VersionTracker
  {
    get => Set<FooView.VersionTracker>();
  }

  public FooDbContext(DbContextOptions<FooDbContext> options)
    : base(options) { }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.ApplyPostgreSQLEventSourcingConfiguration(excludeFromMigrations: true);

    modelBuilder.HasDefaultSchema(FooConstants.DbSchema);
    modelBuilder.ApplyConfiguration(new FooViewVersionTrackerEntityConfiguration());
  }
}

public class FooViewVersionTrackerPersistentProjection(
  IDbContextFactory<FooDbContext> dbContextFactory,
  IDomainEventStore<FooEvent> eventStore,
  ILogger<FooViewVersionTrackerPersistentProjection> logger,
  [FromKeyedServices(FooConstants.VersionTrackerView)] PersistentProjectionOptions projectionOptions
)
  : EntityFrameworkCorePersistentProjection<FooDbContext, FooView.VersionTracker, FooEvent>(
    dbContextFactory,
    eventStore,
    logger,
    projectionOptions
  ) { }

public class FooViewVersionTrackerStrongConsistentProjection(ILogger<FooViewVersionTrackerStrongConsistentProjection> logger)
  : IDbStrongConsistentProjection<FooEvent, FooDbContext>
{
  private readonly ILogger<FooViewVersionTrackerStrongConsistentProjection> _logger = logger;
  private FooView.VersionTracker? _item = null;

  async Task IDbStrongConsistentProjection<FooEvent, FooDbContext>.ApplyEvents(
    IDomainEventStore<FooEvent> eventStore,
    FooDbContext dbContext,
    IReadOnlyList<DomainEvent> domainEvents
  )
  {
    _item = await dbContext
      .UpdateProjectionAsync(eventStore, domainEvents, _item, FooConstants.VersionTrackerView, _logger)
      .ConfigureAwait(false);
  }
}

public static class FooExtensions
{
  extension(EventSourcingJsonSerializerOptions jsonOptions)
  {
    public EventSourcingJsonSerializerOptions AddFooJsonOptions() =>
      jsonOptions.Add(FooJsonSerializerContext.Default).Add<FooEvent>(FooConstants.EventBase);
  }

  extension(IServiceCollection services)
  {
    public IServiceCollection AddFooPostgreSQLPersistence(Func<FooPostgreSQLPersistenceOptions> configureOptions)
    {
      var storageOptions = configureOptions();
      services.AddSingleton(storageOptions);

      services.AddPooledDbContextFactory<FooDbContext>(
        (serviceProvider, dbOptions) =>
        {
          var rootConfiguration = serviceProvider.GetRequiredService<IConfiguration>();
          var connectionString = rootConfiguration.GetConnectionString(storageOptions.ConnectionName);

          dbOptions.UseNpgsql(connectionString, npgsqlOptions =>
          {
            npgsqlOptions.MigrationsAssembly(Assembly.GetExecutingAssembly());
            npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, FooConstants.DbSchema);
            npgsqlOptions.EnableRetryOnFailure();
          });
        }
      );

      services.AddEntityFrameworkCoreDbMigration<FooDbContext>();
      services.AddEntityFrameworkCorePersistentProjection<
        FooViewVersionTrackerPersistentProjection,
        FooDbContext,
        FooView.VersionTracker,
        FooEvent
      >(FooConstants.VersionTrackerView, FooView.VersionTracker.ProjectionVersion);
      services.AddEntityFrameworkCoreStrongConsistentProjection<FooEvent, FooDbContext, FooViewVersionTrackerStrongConsistentProjection>();

      return services;
    }
  }

  extension(EntityFrameworkCoreDomainEventStoreBuilder builder)
  {
    public EntityFrameworkCoreDomainEventStoreBuilder ForFooEvents()
    {
      return builder.ForDomainEvents<FooEvent, FooDbContext>();
    }
  }
}
