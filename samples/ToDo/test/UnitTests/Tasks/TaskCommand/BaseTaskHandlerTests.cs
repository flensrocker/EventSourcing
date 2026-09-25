using Flensrocker.EventSourcing.Abstractions;
using Flensrocker.ToDo.Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;

namespace Flensrocker.ToDo.UnitTests.Tasks.TaskCommand;

public abstract class BaseTaskHandlerTests
{
  protected readonly TestContext _testContext;
  protected readonly CancellationToken _cancellationToken;
  protected readonly FakeTimeProvider _timeProvider = new();
  protected IServiceProvider _serviceProvider = default!;

  protected BaseTaskHandlerTests(TestContext testContext)
  {
    _testContext = testContext;
    _cancellationToken = testContext.CancellationToken;
  }

  [TestInitialize]
  public void Initialize()
  {
    var services = new ServiceCollection();
    services.AddSingleton<TimeProvider>(_timeProvider);
    services.AddToDo();

    _serviceProvider = services.BuildServiceProvider();
  }

  [TestCleanup]
  public async Task Cleanup()
  {
    await ((IAsyncDisposable)_serviceProvider).DisposeAsync();
  }

  protected DomainEvent CreateTaskEvent(Guid taskId, int taskVersion, Abstractions.Tasks.TaskEvent payload)
  {
    return TaskTestUtilities.CreateTaskEvent(_timeProvider, null, null, taskId, taskVersion, payload);
  }
}
