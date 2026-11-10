using System.Linq.Expressions;
using System.Text.Json.Serialization;
using Flensrocker.EventSourcing.Abstractions;
using Flensrocker.EventSourcing.Abstractions.PersistentProjection;
using Flensrocker.EventSourcing.Extensions.AspNetCore;
using Flensrocker.EventSourcing.Extensions.EntityFrameworkCore;
using Flensrocker.EventSourcing.Persistence.EntityFrameworkCore;
using Flensrocker.EventSourcing.Persistence.PostgreSQL;
using Flensrocker.EventSourcing.Test.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;

namespace Flensrocker.EventSourcing.UnitTests.Extensions.EntityFrameworkCore;

[TestClass]
public class EntityFrameworkCoreExtensionsTests
{
  private static ServiceCollection _services = default!;
  private static ServiceProvider _serviceProvider = default!;
  private static readonly FakeTimeProvider _timeProvider = new();

  public TestContext TestContext { get; set; }

  [ClassInitialize]
  public static async Task ClassInitialize(TestContext testContext)
  {
    await TestPostgreSQLContainer.InitializeAsync(testContext.CancellationToken);

    var connectionString = TestPostgreSQLContainer.GetConnectionString();
    var globalJsonOptions = new EventSourcingJsonSerializerOptions()
      .AddAspNetCoreJsonOptions()
      .Add(FooJsonSerializerContext.Default)
      .Add<FooEvent>(FooConstants.EventBase);

    var configuration = new ConfigurationBuilder()
      .AddInMemoryCollection([new("ConnectionStrings:DefaultConnection", connectionString)])
      .Build();

    _services = new ServiceCollection();

    _services.AddSingleton<IConfiguration>(configuration);
    _services.AddLogging();
    _services.AddSingleton<TimeProvider>(_timeProvider);

    _services
      .AddFlensrockerEventSourcingCore()
      .AddPostgreSQLDomainEventStorage(() =>
        new EntityFrameworkCoreDomainEventStorageOptions { ConnectionName = "DefaultConnection", JsonOptions = globalJsonOptions }
      )
      .ForDomainEvents<FooEvent, FooDbContext>();

    _services.AddPooledDbContextFactory<FooDbContext>(options =>
    {
      options.UseNpgsql(connectionString);
    });

    // _services.AddEntityFrameworkCorePersistentProjection<
    //   FooViewVersionTrackerPersistentProjection,
    //   FooDbContext,
    //   FooView.VersionTracker,
    //   FooEvent
    // >(FooConstants.VersionTrackerView, FooView.VersionTracker.ProjectionVersion);
    _services.AddEntityFrameworkCoreStrongConsistentProjection<FooEvent, FooDbContext, FooViewVersionTrackerStrongConsistentProjection>();

    _serviceProvider = _services.BuildServiceProvider();

    await using var setupScope = _serviceProvider.CreateAsyncScope();

    var fooDbContext = setupScope.ServiceProvider.GetRequiredService<FooDbContext>();
    await fooDbContext.Database.EnsureCreatedAsync(testContext.CancellationToken);

    var eventSourcingDbContext = setupScope.ServiceProvider.GetRequiredService<EventSourcingDbContext>();
    await eventSourcingDbContext.Database.MigrateAsync(testContext.CancellationToken);
  }

  [ClassCleanup]
  public static async Task ClassCleanup()
  {
    await _serviceProvider.DisposeAsync();
  }

  private AsyncServiceScope _testScope = default!;

  [TestInitialize]
  public void TestInitialize()
  {
    _testScope = _serviceProvider.CreateAsyncScope();
  }

  [TestCleanup]
  public async Task TestCleanup()
  {
    await _testScope.DisposeAsync();
  }

  [TestMethod]
  public async Task GivenNewIdThenEventStoreAndProjectionAreEmpty()
  {
    var fooId = Guid.CreateVersion7();

    var eventStore = _testScope.ServiceProvider.GetRequiredService<IDomainEventStorage<FooEvent>>();
    var fooDbContext = _testScope.ServiceProvider.GetRequiredService<FooDbContext>();

    var fooEvents = await eventStore.ReadEventsFromStorage(fooId, 1, -1, TestContext.CancellationToken);
    Assert.IsNull(fooEvents);

    var fooVersionTracker = await fooDbContext
      .VersionTracker.Where(foo => foo.FooId == fooId)
      .SingleOrDefaultAsync(TestContext.CancellationToken);
    Assert.IsNull(fooVersionTracker);
  }

  [TestMethod]
  public async Task GivenNewIdAndStoredEventsThenEventStoreAndProjectionAreNotEmpty()
  {
    var fooId = Guid.CreateVersion7();

    var eventStore = _testScope.ServiceProvider.GetRequiredService<IDomainEventStorage<FooEvent>>();
    var fooDbContext = _testScope.ServiceProvider.GetRequiredService<FooDbContext>();

    var createdAt = _timeProvider.GetUtcNow();
    var createdEvent = DomainEvent.Create<FooEvent>(FooConstants.EventBase, fooId, 1, new FooEvent.Created(), null, null, createdAt);

    _timeProvider.Advance(TimeSpan.FromMilliseconds(1L));
    var updatedAt = _timeProvider.GetUtcNow();
    var updatedEvent = DomainEvent.Create<FooEvent>(FooConstants.EventBase, fooId, 2, new FooEvent.Updated(), null, null, updatedAt);

    var updateOk = await eventStore.ApplyUpdatesToStorage(fooId, [createdEvent, updatedEvent], 0);
    Assert.IsTrue(updateOk);

    var fooEvents = await eventStore.ReadEventsFromStorage(fooId, 1, -1, TestContext.CancellationToken);
    Assert.IsNotNull(fooEvents);
    Assert.HasCount(2, fooEvents);

    var fooVersionTracker = await fooDbContext
      .VersionTracker.Where(foo => foo.FooId == fooId)
      .SingleOrDefaultAsync(TestContext.CancellationToken);
    Assert.IsNotNull(fooVersionTracker);
    Assert.AreEqual(fooId, fooVersionTracker.FooId);
    Assert.AreEqual(2, fooVersionTracker.Version);
    Assert.AreEqual("1,2", fooVersionTracker.AppliedVersions);
  }
}

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
    public const int ProjectionVersion = 1;

    [Id(0)]
    [JsonInclude]
    public Guid FooId { get; internal set; }

    [Id(1)]
    [JsonInclude]
    public int Version { get; internal set; }

    [Id(2)]
    [JsonInclude]
    public string AppliedVersions { get; internal set; } = "";

    static object IWhereKey<VersionTracker>.GetKey(DomainEvent domainEvent) => domainEvent.Metadata.StreamId;

    static Expression<Func<VersionTracker, bool>> IWhereKey<VersionTracker>.GetWhereKey(DomainEvent domainEvent) =>
      item => item.FooId == domainEvent.Metadata.StreamId;

    void IDomainEventProjectable.Apply(DomainEvent @event)
    {
      FooId = @event.Metadata.StreamId;
      Version = @event.Metadata.StreamVersion;
      AppliedVersions = string.IsNullOrWhiteSpace(AppliedVersions) ? $"{Version}" : string.Join(",", AppliedVersions, $"{Version}");
    }
  }
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

// public class FooViewVersionTrackerPersistentProjection(
//   IDbContextFactory<FooDbContext> dbContextFactory,
//   IDomainEventStorage<FooEvent> eventStore,
//   ILogger<FooViewVersionTrackerPersistentProjection> logger,
//   [FromKeyedServices(FooConstants.VersionTrackerView)] PersistentProjectionOptions projectionOptions
// )
//   : EntityFrameworkCorePersistentProjection<FooDbContext, FooView.VersionTracker, FooEvent>(
//     dbContextFactory,
//     eventStore,
//     logger,
//     projectionOptions
//   ) { }

public class FooViewVersionTrackerStrongConsistentProjection(ILogger<FooViewVersionTrackerStrongConsistentProjection> logger)
  : IDbStrongConsistentProjection<FooEvent, FooDbContext>
{
  private readonly ILogger<FooViewVersionTrackerStrongConsistentProjection> _logger = logger;
  private FooView.VersionTracker? _item = null;

  async Task IDbStrongConsistentProjection<FooEvent, FooDbContext>.ApplyEvents(
    IDomainEventStorage<FooEvent> eventStore,
    FooDbContext dbContext,
    IReadOnlyList<DomainEvent> domainEvents
  )
  {
    _item = await dbContext
      .UpdateProjectionAsync(eventStore, domainEvents, _item, FooConstants.VersionTrackerView, _logger)
      .ConfigureAwait(false);
  }
}
