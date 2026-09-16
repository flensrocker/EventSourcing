using Flensrocker.EventSourcing.Abstractions;
using Flensrocker.ToDo.Abstractions.Tasks;
using Flensrocker.ToDo.Domain;
using Flensrocker.ToDo.Domain.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using SuT = Flensrocker.ToDo.Abstractions.Tasks.TaskCommand;

namespace Flensrocker.ToDo.UnitTests.Tasks.TaskCommand;

[TestClass]
public sealed class NoteHandlerTests
{
  private readonly FakeTimeProvider _timeProvider = new();
  private readonly string _validTitle = "Valid Title";

  private IServiceProvider _serviceProvider = default!;

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

  [TestMethod]
  public async Task GivenEmptyStateThenReturnNotedEvent()
  {
    var testState = new TaskCommandState();

    await using var testScope = _serviceProvider.CreateAsyncScope();
    var testHandler = testScope.ServiceProvider.GetRequiredService<ICommandHandler<TaskEvent, SuT, TaskCommandState>>();
    var testResult = await SuT
      .Note.Create(title: _validTitle)
      .Pipe(async command => await testHandler.ExecuteCommandAsync(testState, command));

    var result = Assert.IsInstanceOfType<DomainResult<TaskEvent[]>.Success>(testResult);
    Assert.HasCount(1, result.Value);
    var notedEvent = Assert.IsInstanceOfType<TaskEvent.Noted>(result.Value[0]);
    Assert.AreEqual(_timeProvider.GetUtcNow(), notedEvent.CreatedAt);
    Assert.AreEqual(_validTitle, notedEvent.Title);
  }

  [TestMethod]
  public async Task GivenNonEmptyStateThenReturnBadRequest()
  {
    var taskId = Guid.CreateVersion7();
    var taskVersion = 1;
    var taskCreatedAt = _timeProvider.GetUtcNow();

    var testState = new TaskCommandState();
    testState.Apply(
      DomainEvent.Create<TaskEvent>(
        streamType: TaskConstants.TaskEvent.Base,
        streamId: taskId,
        streamVersion: taskVersion++,
        payload: new TaskEvent.Noted { CreatedAt = taskCreatedAt, Title = _validTitle },
        tenantId: null,
        userId: null,
        createdAt: taskCreatedAt
      )
    );

    await using var testScope = _serviceProvider.CreateAsyncScope();
    var testHandler = testScope.ServiceProvider.GetRequiredService<ICommandHandler<TaskEvent, SuT, TaskCommandState>>();
    var testResult = await SuT
      .Note.Create(title: _validTitle)
      .Pipe(async command => await testHandler.ExecuteCommandAsync(testState, command));

    Assert.IsInstanceOfType<DomainResult<TaskEvent[]>.BadRequest>(testResult);
  }
}
