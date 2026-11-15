using Flensrocker.EventSourcing.Abstractions;
using Flensrocker.EventSourcing.Abstractions.PersistentProjection;
using Flensrocker.EventSourcing.Extensions.AspNetCore;
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
  private ServiceCollection _services = default!;
  private ServiceProvider _serviceProvider = default!;

  public TestContext TestContext { get; set; }

  [TestInitialize]
  public async Task TestInitialize()
  {
    await TestPostgreSQLContainer.InitializeAsync(TestContext.CancellationToken);

    var connectionString = TestPostgreSQLContainer.GetConnectionString();
    // TODO think about making this injectable
    var globalJsonOptions = new EventSourcingJsonSerializerOptions().AddAspNetCoreJsonOptions().AddFooJsonOptions();

    var configuration = new ConfigurationBuilder()
      .AddInMemoryCollection([new("ConnectionStrings:DefaultConnection", connectionString)])
      .Build();

    _services = new ServiceCollection();

    _services.AddSingleton<IConfiguration>(configuration);
    _services.AddLogging();
    _services.AddSingleton<FakeTimeProvider>();
    _services.AddSingleton<TimeProvider>(sp => sp.GetRequiredService<FakeTimeProvider>());

    _services
      .AddFlensrockerEventSourcingCore()
      .AddPostgreSQLDomainEventStorage(() =>
        new EntityFrameworkCoreDomainEventStorageOptions { ConnectionName = "DefaultConnection", JsonOptions = globalJsonOptions }
      )
      .ForFooEvents();

    _services.AddFooPostgreSQLPersistence(() => new FooPostgreSQLPersistenceOptions { ConnectionName = "DefaultConnection" });

    _serviceProvider = _services.BuildServiceProvider();

    await using var setupScope = _serviceProvider.CreateAsyncScope();

    var fooDbContext = setupScope.ServiceProvider.GetRequiredService<FooDbContext>();
    await fooDbContext.Database.EnsureCreatedAsync(TestContext.CancellationToken);

    var eventSourcingDbContext = setupScope.ServiceProvider.GetRequiredService<EventSourcingDbContext>();
    await eventSourcingDbContext.Database.MigrateAsync(TestContext.CancellationToken);
  }

  [TestCleanup]
  public async Task TestCleanup()
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
    var tenantId = Guid.CreateVersion7();
    var userId = Guid.CreateVersion7();
    var fooId = Guid.CreateVersion7();
    var fooVersion = 0;

    await using (var arrangeScope = _serviceProvider.CreateAsyncScope())
    {
      var timeProvider = arrangeScope.ServiceProvider.GetRequiredService<FakeTimeProvider>();
      var eventStore = arrangeScope.ServiceProvider.GetRequiredService<IDomainEventStorage<FooEvent>>();
      var fooDbContext = arrangeScope.ServiceProvider.GetRequiredService<FooDbContext>();
      var expectedFooVersion = fooVersion;

      var createdAt = timeProvider.GetUtcNow();
      var createdEvent = DomainEvent.Create<FooEvent>(
        FooConstants.EventBase,
        fooId,
        ++fooVersion,
        new FooEvent.Created(),
        tenantId,
        userId,
        createdAt
      );

      timeProvider.Advance(TimeSpan.FromMilliseconds(1L));
      var updatedAt = timeProvider.GetUtcNow();
      var updatedEvent = DomainEvent.Create<FooEvent>(
        FooConstants.EventBase,
        fooId,
        ++fooVersion,
        new FooEvent.Updated(),
        tenantId,
        userId,
        updatedAt
      );

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
      Assert.AreEqual(tenantId, fooVersionTracker.TenantId);
      Assert.AreEqual("1,2", fooVersionTracker.AppliedVersions);
    }
  }

  [TestMethod]
  public async Task GivenNewIdAndOutOfDateProjectionThenMissingEventsAreAppliedToProjection()
  {
    var tenantId = Guid.CreateVersion7();
    var userId = Guid.CreateVersion7();
    var fooId = Guid.CreateVersion7();
    var fooVersion = 0;

    await using (var arrangeScope = _serviceProvider.CreateAsyncScope())
    {
      var timeProvider = arrangeScope.ServiceProvider.GetRequiredService<FakeTimeProvider>();
      var eventStore = arrangeScope.ServiceProvider.GetRequiredService<IDomainEventStorage<FooEvent>>();
      var fooDbContext = arrangeScope.ServiceProvider.GetRequiredService<FooDbContext>();
      var expectedFooVersion = fooVersion;

      var createdAt = timeProvider.GetUtcNow();
      var createdEvent = DomainEvent.Create<FooEvent>(
        FooConstants.EventBase,
        fooId,
        ++fooVersion,
        new FooEvent.Created(),
        tenantId,
        userId,
        createdAt
      );

      timeProvider.Advance(TimeSpan.FromMilliseconds(1L));
      var updated1At = timeProvider.GetUtcNow();
      var updated1Event = DomainEvent.Create<FooEvent>(
        FooConstants.EventBase,
        fooId,
        ++fooVersion,
        new FooEvent.Updated(),
        tenantId,
        userId,
        updated1At
      );

      var firstUpdateOk = await eventStore.ApplyUpdatesToStorage(fooId, [createdEvent, updated1Event], expectedFooVersion);
      Assert.IsTrue(firstUpdateOk);

      var fooVersionTracker = await fooDbContext
        .VersionTracker.Where(foo => foo.FooId == fooId)
        .SingleOrDefaultAsync(TestContext.CancellationToken);
      Assert.IsNotNull(fooVersionTracker);
      Assert.AreEqual(fooId, fooVersionTracker.FooId);
      Assert.AreEqual(fooVersion, fooVersionTracker.Version);
      Assert.AreEqual(tenantId, fooVersionTracker.TenantId);
      Assert.AreEqual("1,2", fooVersionTracker.AppliedVersions);

      fooVersionTracker.Version = 1;
      fooVersionTracker.AppliedVersions = "1";
      await fooDbContext.SaveChangesAsync(TestContext.CancellationToken);
    }

    await using (var assertScope = _serviceProvider.CreateAsyncScope())
    {
      var timeProvider = assertScope.ServiceProvider.GetRequiredService<FakeTimeProvider>();
      var eventStore = assertScope.ServiceProvider.GetRequiredService<IDomainEventStorage<FooEvent>>();
      var fooDbContext = assertScope.ServiceProvider.GetRequiredService<FooDbContext>();
      var expectedFooVersion = fooVersion;

      timeProvider.Advance(TimeSpan.FromMilliseconds(1L));
      var updated2At = timeProvider.GetUtcNow();
      var updated2Event = DomainEvent.Create<FooEvent>(
        FooConstants.EventBase,
        fooId,
        ++fooVersion,
        new FooEvent.Updated(),
        tenantId,
        userId,
        updated2At
      );

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
      Assert.AreEqual(tenantId, fooVersionTracker.TenantId);
      Assert.AreEqual("1,2,3", fooVersionTracker.AppliedVersions);
    }
  }

  [TestMethod]
  public async Task GivenEventsAndDeletedProjectionThenTheProjectionCanBeRebuilt()
  {
    var tenantId = Guid.CreateVersion7();
    var userId = Guid.CreateVersion7();
    var fooId = Guid.CreateVersion7();
    var fooVersion = 0;

    await using (var arrangeScope = _serviceProvider.CreateAsyncScope())
    {
      var timeProvider = arrangeScope.ServiceProvider.GetRequiredService<FakeTimeProvider>();
      var eventStore = arrangeScope.ServiceProvider.GetRequiredService<IDomainEventStorage<FooEvent>>();
      var fooDbContext = arrangeScope.ServiceProvider.GetRequiredService<FooDbContext>();
      var expectedFooVersion = fooVersion;

      var createdAt = timeProvider.GetUtcNow();
      var createdEvent = DomainEvent.Create<FooEvent>(
        FooConstants.EventBase,
        fooId,
        ++fooVersion,
        new FooEvent.Created(),
        tenantId,
        userId,
        createdAt
      );

      timeProvider.Advance(TimeSpan.FromMilliseconds(1L));
      var updated1At = timeProvider.GetUtcNow();
      var updated1Event = DomainEvent.Create<FooEvent>(
        FooConstants.EventBase,
        fooId,
        ++fooVersion,
        new FooEvent.Updated(),
        tenantId,
        userId,
        updated1At
      );

      timeProvider.Advance(TimeSpan.FromMilliseconds(1L));
      var updated2At = timeProvider.GetUtcNow();
      var updated2Event = DomainEvent.Create<FooEvent>(
        FooConstants.EventBase,
        fooId,
        ++fooVersion,
        new FooEvent.Updated(),
        tenantId,
        userId,
        updated2At
      );

      var updateOk = await eventStore.ApplyUpdatesToStorage(fooId, [createdEvent, updated1Event, updated2Event], expectedFooVersion);
      Assert.IsTrue(updateOk);

      var fooVersionTracker = await fooDbContext
        .VersionTracker.Where(foo => foo.FooId == fooId)
        .SingleOrDefaultAsync(TestContext.CancellationToken);
      Assert.IsNotNull(fooVersionTracker);
      Assert.AreEqual(fooId, fooVersionTracker.FooId);
      Assert.AreEqual(fooVersion, fooVersionTracker.Version);
      Assert.AreEqual(tenantId, fooVersionTracker.TenantId);
      Assert.AreEqual("1,2,3", fooVersionTracker.AppliedVersions);

      fooDbContext.VersionTracker.Remove(fooVersionTracker);
      await fooDbContext.SaveChangesAsync(TestContext.CancellationToken);
    }

    await using (var actScope = _serviceProvider.CreateAsyncScope())
    {
      var projection = actScope.ServiceProvider.GetRequiredKeyedService<IPersistentProjection>(FooConstants.VersionTrackerView);

      await projection.RebuildAsync();
    }

    await using (var assertScope = _serviceProvider.CreateAsyncScope())
    {
      var fooDbContext = assertScope.ServiceProvider.GetRequiredService<FooDbContext>();

      var fooVersionTracker = await fooDbContext
        .VersionTracker.Where(foo => foo.FooId == fooId)
        .SingleOrDefaultAsync(TestContext.CancellationToken);
      Assert.IsNotNull(fooVersionTracker);
      Assert.AreEqual(fooId, fooVersionTracker.FooId);
      Assert.AreEqual(fooVersion, fooVersionTracker.Version);
      Assert.AreEqual(tenantId, fooVersionTracker.TenantId);
      Assert.AreEqual("1,2,3", fooVersionTracker.AppliedVersions);
    }
  }
}
