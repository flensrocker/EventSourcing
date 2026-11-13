using Flensrocker.EventSourcing.Abstractions;
using Flensrocker.EventSourcing.Extensions.AspNetCore;
using Flensrocker.EventSourcing.Extensions.EntityFrameworkCore;
using Flensrocker.EventSourcing.Persistence.EntityFrameworkCore;
using Flensrocker.EventSourcing.Persistence.PostgreSQL;
using Flensrocker.EventSourcing.Test.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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

  [TestMethod]
  public async Task GivenNewIdThenEventStoreAndProjectionAreEmpty()
  {
    var fooId = Guid.CreateVersion7();

    await using (var testScope = _serviceProvider.CreateAsyncScope())
    {
      var eventStore = testScope.ServiceProvider.GetRequiredService<IDomainEventStorage<FooEvent>>();
      var fooDbContext = testScope.ServiceProvider.GetRequiredService<FooDbContext>();

      var fooEvents = await eventStore.ReadEventsFromStorage(fooId, 1, -1, TestContext.CancellationToken);
      Assert.IsNull(fooEvents);

      var fooVersionTracker = await fooDbContext
        .VersionTracker.Where(foo => foo.FooId == fooId)
        .SingleOrDefaultAsync(TestContext.CancellationToken);
      Assert.IsNull(fooVersionTracker);
    }
  }

  [TestMethod]
  public async Task GivenNewIdAndStoredEventsThenEventStoreAndProjectionAreNotEmpty()
  {
    var fooId = Guid.CreateVersion7();
    var fooVersion = 0;

    await using (var arrangeScope = _serviceProvider.CreateAsyncScope())
    {
      var eventStore = arrangeScope.ServiceProvider.GetRequiredService<IDomainEventStorage<FooEvent>>();
      var fooDbContext = arrangeScope.ServiceProvider.GetRequiredService<FooDbContext>();
      var expectedFooVersion = fooVersion;

      var createdAt = _timeProvider.GetUtcNow();
      var createdEvent = DomainEvent.Create<FooEvent>(FooConstants.EventBase, fooId, ++fooVersion, new FooEvent.Created(), null, null, createdAt);

      _timeProvider.Advance(TimeSpan.FromMilliseconds(1L));
      var updatedAt = _timeProvider.GetUtcNow();
      var updatedEvent = DomainEvent.Create<FooEvent>(FooConstants.EventBase, fooId, ++fooVersion, new FooEvent.Updated(), null, null, updatedAt);

      var updateOk = await eventStore.ApplyUpdatesToStorage(fooId, [createdEvent, updatedEvent], expectedFooVersion);
      Assert.IsTrue(updateOk);
    }

    await using (var assertScope = _serviceProvider.CreateAsyncScope())
    {
      var eventStore = assertScope.ServiceProvider.GetRequiredService<IDomainEventStorage<FooEvent>>();
      var fooDbContext = assertScope.ServiceProvider.GetRequiredService<FooDbContext>();

      var fooEvents = await eventStore.ReadEventsFromStorage(fooId, 1, -1, TestContext.CancellationToken);
      Assert.IsNotNull(fooEvents);
      Assert.HasCount(fooVersion, fooEvents);

      var fooVersionTracker = await fooDbContext
        .VersionTracker.Where(foo => foo.FooId == fooId)
        .SingleOrDefaultAsync(TestContext.CancellationToken);
      Assert.IsNotNull(fooVersionTracker);
      Assert.AreEqual(fooId, fooVersionTracker.FooId);
      Assert.AreEqual(fooVersion, fooVersionTracker.Version);
      Assert.AreEqual("1,2", fooVersionTracker.AppliedVersions);
    }
  }

  [TestMethod]
  public async Task GivenNewIdAndOutOfDateProjectionThenMissingEventsAreAppliedToProjection()
  {
    var fooId = Guid.CreateVersion7();
    var fooVersion = 0;

    await using (var arrangeScope = _serviceProvider.CreateAsyncScope())
    {
      var eventStore = arrangeScope.ServiceProvider.GetRequiredService<IDomainEventStorage<FooEvent>>();
      var fooDbContext = arrangeScope.ServiceProvider.GetRequiredService<FooDbContext>();
      var expectedFooVersion = fooVersion;

      var createdAt = _timeProvider.GetUtcNow();
      var createdEvent = DomainEvent.Create<FooEvent>(FooConstants.EventBase, fooId, ++fooVersion, new FooEvent.Created(), null, null, createdAt);

      _timeProvider.Advance(TimeSpan.FromMilliseconds(1L));
      var updated1At = _timeProvider.GetUtcNow();
      var updated1Event = DomainEvent.Create<FooEvent>(FooConstants.EventBase, fooId, ++fooVersion, new FooEvent.Updated(), null, null, updated1At);

      var firstUpdateOk = await eventStore.ApplyUpdatesToStorage(fooId, [createdEvent, updated1Event], expectedFooVersion);
      Assert.IsTrue(firstUpdateOk);

      var fooVersionTracker = await fooDbContext
        .VersionTracker.Where(foo => foo.FooId == fooId)
        .SingleOrDefaultAsync(TestContext.CancellationToken);
      Assert.IsNotNull(fooVersionTracker);
      Assert.AreEqual(fooId, fooVersionTracker.FooId);
      Assert.AreEqual(fooVersion, fooVersionTracker.Version);
      Assert.AreEqual("1,2", fooVersionTracker.AppliedVersions);

      fooVersionTracker.Version = 1;
      fooVersionTracker.AppliedVersions = "1";
      await fooDbContext.SaveChangesAsync(TestContext.CancellationToken);
    }

    await using (var assertScope = _serviceProvider.CreateAsyncScope())
    {
      var eventStore = assertScope.ServiceProvider.GetRequiredService<IDomainEventStorage<FooEvent>>();
      var fooDbContext = assertScope.ServiceProvider.GetRequiredService<FooDbContext>();
      var expectedFooVersion = fooVersion;

      _timeProvider.Advance(TimeSpan.FromMilliseconds(1L));
      var updated2At = _timeProvider.GetUtcNow();
      var updated2Event = DomainEvent.Create<FooEvent>(FooConstants.EventBase, fooId, ++fooVersion, new FooEvent.Updated(), null, null, updated2At);

      var secondUpdateOk = await eventStore.ApplyUpdatesToStorage(fooId, [updated2Event], expectedFooVersion);
      Assert.IsTrue(secondUpdateOk);

      var fooEvents = await eventStore.ReadEventsFromStorage(fooId, 1, -1, TestContext.CancellationToken);
      Assert.IsNotNull(fooEvents);
      Assert.HasCount(fooVersion, fooEvents);

      var fooVersionTracker = await fooDbContext
        .VersionTracker.Where(foo => foo.FooId == fooId)
        .SingleOrDefaultAsync(TestContext.CancellationToken);
      Assert.IsNotNull(fooVersionTracker);
      Assert.AreEqual(fooId, fooVersionTracker.FooId);
      Assert.AreEqual(fooVersion, fooVersionTracker.Version);
      Assert.AreEqual("1,2,3", fooVersionTracker.AppliedVersions);
    }
  }
}
