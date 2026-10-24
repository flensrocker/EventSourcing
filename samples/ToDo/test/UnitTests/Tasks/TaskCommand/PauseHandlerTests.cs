using Flensrocker.EventSourcing.Abstractions;
using Flensrocker.ToDo.Domain.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SuT = Flensrocker.ToDo.Abstractions.Tasks.TaskCommand;

namespace Flensrocker.ToDo.UnitTests.Tasks.TaskCommand;

[TestClass]
public sealed class PauseHandlerTests : BaseTaskHandlerTests
{
  public PauseHandlerTests(TestContext testContext)
    : base(testContext) { }

  [TestMethod]
  public async Task GivenStartedStateThenReturnPausedEvent()
  {
    var userId = Guid.CreateVersion7();
    var taskId = Guid.CreateVersion7();
    var taskVersion = 1;
    var taskCreatedAt = _timeProvider.GetUtcNow();
    var taskStartedAt = taskCreatedAt.AddMinutes(1.0);
    var taskPausedAt = taskCreatedAt.AddMinutes(1.0);

    var testState = new TaskCommandState();
    testState.Apply(
      CreateTaskEvent(taskId, taskVersion++, new Abstractions.Tasks.TaskEvent.Noted { CreatedAt = taskCreatedAt, Title = "Some Title" })
    );
    testState.Apply(
      CreateTaskEvent(taskId, taskVersion++, new Abstractions.Tasks.TaskEvent.Started { StartedAt = taskStartedAt, ByUserId = userId })
    );

    await using var testScope = _serviceProvider.CreateAsyncScope();
    var testHandler = testScope.ServiceProvider.GetRequiredService<ICommandHandler<Abstractions.Tasks.TaskEvent, SuT, TaskCommandState>>();
    var testResult = await SuT
      .Pause.Create(pausedAt: taskPausedAt)
      .Pipe(async command => await testHandler.ExecuteCommandAsync(testState, command).ConfigureAwait(false))
      .ConfigureAwait(false);

    var result = Assert.IsInstanceOfType<DomainResult<Abstractions.Tasks.TaskEvent[]>.Success>(testResult);
    Assert.HasCount(1, result.Value);
    var pausedEvent = Assert.IsInstanceOfType<Abstractions.Tasks.TaskEvent.Paused>(result.Value[0]);
    Assert.AreEqual(taskPausedAt, pausedEvent.PausedAt);
  }

  [TestMethod]
  public async Task GivenPausedStateThenReturnEmptySuccess()
  {
    var userId = Guid.CreateVersion7();
    var taskId = Guid.CreateVersion7();
    var taskVersion = 1;
    var taskCreatedAt = _timeProvider.GetUtcNow();
    var taskStartedAt = taskCreatedAt.AddMinutes(1.0);
    var taskPausedAt = taskStartedAt.AddMinutes(1.0);

    var testState = new TaskCommandState();
    testState.Apply(
      CreateTaskEvent(taskId, taskVersion++, new Abstractions.Tasks.TaskEvent.Noted { CreatedAt = taskCreatedAt, Title = "Some Title" })
    );
    testState.Apply(
      CreateTaskEvent(taskId, taskVersion++, new Abstractions.Tasks.TaskEvent.Started { StartedAt = taskStartedAt, ByUserId = userId })
    );
    testState.Apply(CreateTaskEvent(taskId, taskVersion++, new Abstractions.Tasks.TaskEvent.Paused { PausedAt = taskPausedAt }));

    await using var testScope = _serviceProvider.CreateAsyncScope();
    var testHandler = testScope.ServiceProvider.GetRequiredService<ICommandHandler<Abstractions.Tasks.TaskEvent, SuT, TaskCommandState>>();
    var testResult = await SuT
      .Pause.Create(pausedAt: taskPausedAt)
      .Pipe(async command => await testHandler.ExecuteCommandAsync(testState, command).ConfigureAwait(false))
      .ConfigureAwait(false);

    var result = Assert.IsInstanceOfType<DomainResult<Abstractions.Tasks.TaskEvent[]>.Success>(testResult);
    Assert.IsEmpty(result.Value);
  }

  [TestMethod]
  public async Task GivenDismissedStateThenReturnBadRequest()
  {
    var taskId = Guid.CreateVersion7();
    var taskVersion = 1;
    var taskCreatedAt = _timeProvider.GetUtcNow();
    var taskPausedAt = taskCreatedAt.AddMinutes(1.0);

    var testState = new TaskCommandState();
    testState.Apply(
      CreateTaskEvent(taskId, taskVersion++, new Abstractions.Tasks.TaskEvent.Noted { CreatedAt = taskCreatedAt, Title = "Some Title" })
    );
    testState.Apply(CreateTaskEvent(taskId, taskVersion++, new Abstractions.Tasks.TaskEvent.Dismissed { Reason = "Some Reason" }));

    await using var testScope = _serviceProvider.CreateAsyncScope();
    var testHandler = testScope.ServiceProvider.GetRequiredService<ICommandHandler<Abstractions.Tasks.TaskEvent, SuT, TaskCommandState>>();
    var testResult = await SuT
      .Pause.Create(pausedAt: taskPausedAt)
      .Pipe(async command => await testHandler.ExecuteCommandAsync(testState, command).ConfigureAwait(false))
      .ConfigureAwait(false);

    Assert.IsInstanceOfType<DomainResult<Abstractions.Tasks.TaskEvent[]>.BadRequest>(testResult);
  }

  [TestMethod]
  public async Task GivenOpenedStateThenReturnBadRequest()
  {
    var taskId = Guid.CreateVersion7();
    var taskVersion = 1;
    var taskCreatedAt = _timeProvider.GetUtcNow();
    var taskPausedAt = taskCreatedAt.AddMinutes(1.0);

    var testState = new TaskCommandState();
    testState.Apply(
      CreateTaskEvent(taskId, taskVersion++, new Abstractions.Tasks.TaskEvent.Noted { CreatedAt = taskCreatedAt, Title = "Some Title" })
    );

    await using var testScope = _serviceProvider.CreateAsyncScope();
    var testHandler = testScope.ServiceProvider.GetRequiredService<ICommandHandler<Abstractions.Tasks.TaskEvent, SuT, TaskCommandState>>();
    var testResult = await SuT
      .Pause.Create(pausedAt: taskPausedAt)
      .Pipe(async command => await testHandler.ExecuteCommandAsync(testState, command).ConfigureAwait(false))
      .ConfigureAwait(false);

    Assert.IsInstanceOfType<DomainResult<Abstractions.Tasks.TaskEvent[]>.BadRequest>(testResult);
  }

  [TestMethod]
  public async Task GivenCompletedStateThenReturnBadRequest()
  {
    var taskId = Guid.CreateVersion7();
    var taskVersion = 1;
    var taskCreatedAt = _timeProvider.GetUtcNow();
    var taskPausedAt = taskCreatedAt.AddMinutes(1.0);

    var testState = new TaskCommandState();
    testState.Apply(
      CreateTaskEvent(taskId, taskVersion++, new Abstractions.Tasks.TaskEvent.Noted { CreatedAt = taskCreatedAt, Title = "Some Title" })
    );
    testState.Apply(CreateTaskEvent(taskId, taskVersion++, new Abstractions.Tasks.TaskEvent.Completed { }));

    await using var testScope = _serviceProvider.CreateAsyncScope();
    var testHandler = testScope.ServiceProvider.GetRequiredService<ICommandHandler<Abstractions.Tasks.TaskEvent, SuT, TaskCommandState>>();
    var testResult = await SuT
      .Pause.Create(pausedAt: taskPausedAt)
      .Pipe(async command => await testHandler.ExecuteCommandAsync(testState, command).ConfigureAwait(false))
      .ConfigureAwait(false);

    Assert.IsInstanceOfType<DomainResult<Abstractions.Tasks.TaskEvent[]>.BadRequest>(testResult);
  }

  [TestMethod]
  public async Task GivenReopenedStateThenReturnBadRequest()
  {
    var taskId = Guid.CreateVersion7();
    var taskVersion = 1;
    var taskCreatedAt = _timeProvider.GetUtcNow();
    var taskPausedAt = taskCreatedAt.AddMinutes(1.0);

    var testState = new TaskCommandState();
    testState.Apply(
      CreateTaskEvent(taskId, taskVersion++, new Abstractions.Tasks.TaskEvent.Noted { CreatedAt = taskCreatedAt, Title = "Some Title" })
    );
    testState.Apply(CreateTaskEvent(taskId, taskVersion++, new Abstractions.Tasks.TaskEvent.Completed { }));
    testState.Apply(CreateTaskEvent(taskId, taskVersion++, new Abstractions.Tasks.TaskEvent.Reopened { }));

    await using var testScope = _serviceProvider.CreateAsyncScope();
    var testHandler = testScope.ServiceProvider.GetRequiredService<ICommandHandler<Abstractions.Tasks.TaskEvent, SuT, TaskCommandState>>();
    var testResult = await SuT
      .Pause.Create(pausedAt: taskPausedAt)
      .Pipe(async command => await testHandler.ExecuteCommandAsync(testState, command).ConfigureAwait(false))
      .ConfigureAwait(false);

    Assert.IsInstanceOfType<DomainResult<Abstractions.Tasks.TaskEvent[]>.BadRequest>(testResult);
  }

  [TestMethod]
  public async Task GivenEmptyStateThenReturnNotFound()
  {
    var taskPausedAt = _timeProvider.GetUtcNow();

    var testState = new TaskCommandState();

    await using var testScope = _serviceProvider.CreateAsyncScope();
    var testHandler = testScope.ServiceProvider.GetRequiredService<ICommandHandler<Abstractions.Tasks.TaskEvent, SuT, TaskCommandState>>();
    var testResult = await SuT
      .Pause.Create(pausedAt: taskPausedAt)
      .Pipe(async command => await testHandler.ExecuteCommandAsync(testState, command).ConfigureAwait(false))
      .ConfigureAwait(false);

    Assert.IsInstanceOfType<DomainResult<Abstractions.Tasks.TaskEvent[]>.NotFound>(testResult);
  }
}
