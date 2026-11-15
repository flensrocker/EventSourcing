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
    await TestPostgreSQLContainer.InitializeAsync(TestContext.CancellationToken).ConfigureAwait(false);

    var connectionString = TestPostgreSQLContainer.GetConnectionString();
    // TODO think about making this injectable
    var globalJsonOptions = new EventSourcingJsonSerializerOptions().AddAspNetCoreJsonOptions().AddFooJsonOptions().MakeReadOnly();

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
    await fooDbContext.Database.EnsureCreatedAsync(TestContext.CancellationToken).ConfigureAwait(false);

    var eventSourcingDbContext = setupScope.ServiceProvider.GetRequiredService<EventSourcingDbContext>();
    await eventSourcingDbContext.Database.MigrateAsync(TestContext.CancellationToken).ConfigureAwait(false);

    await setupScope.ServiceProvider.RebuildOutdatedPersistentProjectionsAsync().ConfigureAwait(false);
  }

  [TestCleanup]
  public async Task TestCleanup()
  {
    await _serviceProvider.DisposeAsync().ConfigureAwait(false);
  }

  [TestMethod]
  public async Task GivenNewIdThenFooEventStoreAndProjectionAreEmptyAndProjectionIsRegistered()
  {
    var fooId = Guid.CreateVersion7();

    await using (var assertScope = _serviceProvider.CreateAsyncScope())
    {
      var projectionEventStore = assertScope.ServiceProvider.GetRequiredService<IDomainEventStorage<PersistentProjectionEvent>>();
      var fooEventStore = assertScope.ServiceProvider.GetRequiredService<IDomainEventStorage<FooEvent>>();
      var fooDbContext = assertScope.ServiceProvider.GetRequiredService<FooDbContext>();

      var projectionEvents = new List<DomainEvent>();
      await foreach (
        var projectionEvent in projectionEventStore.ReadAllEventsFromStorage(TestContext.CancellationToken).ConfigureAwait(false)
      )
      {
        projectionEvents.Add(projectionEvent);
      }
      Assert.IsNotNull(projectionEvents);
      Assert.HasCount(2, projectionEvents);
      var projectionRegisteredEvent = Assert.IsInstanceOfType<PersistentProjectionEvent.ProjectionRegistered>(projectionEvents[0].Payload);
      Assert.AreEqual(FooConstants.VersionTrackerView, projectionRegisteredEvent.TypeName);
      var projectionRebuiltEvent = Assert.IsInstanceOfType<PersistentProjectionEvent.ProjectionRebuilt>(projectionEvents[1].Payload);
      Assert.AreEqual(FooConstants.VersionTrackerView, projectionRebuiltEvent.TypeName);
      Assert.AreEqual(FooView.VersionTracker.ProjectionVersion, projectionRebuiltEvent.Version);

      var fooEvents = await fooEventStore.ReadEventsFromStorage(fooId, 1, -1, TestContext.CancellationToken).ConfigureAwait(false);
      Assert.IsNull(fooEvents);

      var fooVersionTracker = await fooDbContext
        .VersionTracker.Where(foo => foo.FooId == fooId)
        .SingleOrDefaultAsync(TestContext.CancellationToken)
        .ConfigureAwait(false);
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

      var updateOk = await eventStore.ApplyUpdatesToStorage(fooId, [createdEvent, updatedEvent], expectedFooVersion).ConfigureAwait(false);
      Assert.IsTrue(updateOk);
    }

    await using (var assertScope = _serviceProvider.CreateAsyncScope())
    {
      var eventStore = assertScope.ServiceProvider.GetRequiredService<IDomainEventStorage<FooEvent>>();
      var fooDbContext = assertScope.ServiceProvider.GetRequiredService<FooDbContext>();

      var fooEvents = await eventStore.ReadEventsFromStorage(fooId, 1, -1, TestContext.CancellationToken).ConfigureAwait(false);
      Assert.IsNotNull(fooEvents);
      Assert.HasCount(fooVersion, fooEvents);

      var fooVersionTracker = await fooDbContext
        .VersionTracker.Where(foo => foo.FooId == fooId)
        .SingleOrDefaultAsync(TestContext.CancellationToken)
        .ConfigureAwait(false);
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

      var firstUpdateOk = await eventStore
        .ApplyUpdatesToStorage(fooId, [createdEvent, updated1Event], expectedFooVersion)
        .ConfigureAwait(false);
      Assert.IsTrue(firstUpdateOk);

      var fooVersionTracker = await fooDbContext
        .VersionTracker.Where(foo => foo.FooId == fooId)
        .SingleOrDefaultAsync(TestContext.CancellationToken)
        .ConfigureAwait(false);
      Assert.IsNotNull(fooVersionTracker);
      Assert.AreEqual(fooId, fooVersionTracker.FooId);
      Assert.AreEqual(fooVersion, fooVersionTracker.Version);
      Assert.AreEqual(tenantId, fooVersionTracker.TenantId);
      Assert.AreEqual("1,2", fooVersionTracker.AppliedVersions);

      fooVersionTracker.Version = 1;
      fooVersionTracker.AppliedVersions = "1";
      await fooDbContext.SaveChangesAsync(TestContext.CancellationToken).ConfigureAwait(false);
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

      var secondUpdateOk = await eventStore.ApplyUpdatesToStorage(fooId, [updated2Event], expectedFooVersion).ConfigureAwait(false);
      Assert.IsTrue(secondUpdateOk);

      var fooEvents = await eventStore.ReadEventsFromStorage(fooId, 1, -1, TestContext.CancellationToken).ConfigureAwait(false);
      Assert.IsNotNull(fooEvents);
      Assert.HasCount(fooVersion, fooEvents);

      var fooVersionTracker = await fooDbContext
        .VersionTracker.Where(foo => foo.FooId == fooId)
        .SingleOrDefaultAsync(TestContext.CancellationToken)
        .ConfigureAwait(false);
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
      var projectionEventStore = arrangeScope.ServiceProvider.GetRequiredService<IDomainEventStorage<PersistentProjectionEvent>>();
      var fooEventStore = arrangeScope.ServiceProvider.GetRequiredService<IDomainEventStorage<FooEvent>>();
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

      var updateOk = await fooEventStore
        .ApplyUpdatesToStorage(fooId, [createdEvent, updated1Event, updated2Event], expectedFooVersion)
        .ConfigureAwait(false);
      Assert.IsTrue(updateOk);

      var fooVersionTracker = await fooDbContext
        .VersionTracker.Where(foo => foo.FooId == fooId)
        .SingleOrDefaultAsync(TestContext.CancellationToken)
        .ConfigureAwait(false);
      Assert.IsNotNull(fooVersionTracker);
      Assert.AreEqual(fooId, fooVersionTracker.FooId);
      Assert.AreEqual(fooVersion, fooVersionTracker.Version);
      Assert.AreEqual(tenantId, fooVersionTracker.TenantId);
      Assert.AreEqual("1,2,3", fooVersionTracker.AppliedVersions);

      fooDbContext.VersionTracker.Remove(fooVersionTracker);
      await fooDbContext.SaveChangesAsync(TestContext.CancellationToken).ConfigureAwait(false);

      timeProvider.Advance(TimeSpan.FromMilliseconds(1L));
      var rebuiltAt = timeProvider.GetUtcNow();
      var rebuiltEvent = DomainEvent.Create<PersistentProjectionEvent>(
        PersistentProjectionConstants.PersistentProjectionEvent.Base,
        PersistentProjectionConstants.RegistryId,
        3,
        new PersistentProjectionEvent.ProjectionRebuilt
        {
          TypeName = FooConstants.VersionTrackerView,
          Version = FooView.VersionTracker.ProjectionVersion - 1,
        },
        null,
        null,
        rebuiltAt
      );
      updateOk = await projectionEventStore
        .ApplyUpdatesToStorage(PersistentProjectionConstants.RegistryId, [rebuiltEvent], 2)
        .ConfigureAwait(false);
      Assert.IsTrue(updateOk);
    }

    await using (var actScope = _serviceProvider.CreateAsyncScope())
    {
      await actScope.ServiceProvider.RebuildOutdatedPersistentProjectionsAsync().ConfigureAwait(false);
    }

    await using (var assertScope = _serviceProvider.CreateAsyncScope())
    {
      var fooDbContext = assertScope.ServiceProvider.GetRequiredService<FooDbContext>();

      var fooVersionTracker = await fooDbContext
        .VersionTracker.Where(foo => foo.FooId == fooId)
        .SingleOrDefaultAsync(TestContext.CancellationToken)
        .ConfigureAwait(false);
      Assert.IsNotNull(fooVersionTracker);
      Assert.AreEqual(fooId, fooVersionTracker.FooId);
      Assert.AreEqual(fooVersion, fooVersionTracker.Version);
      Assert.AreEqual(tenantId, fooVersionTracker.TenantId);
      Assert.AreEqual("1,2,3", fooVersionTracker.AppliedVersions);
    }
  }
}
