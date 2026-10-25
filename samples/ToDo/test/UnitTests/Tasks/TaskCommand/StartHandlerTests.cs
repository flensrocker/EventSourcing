using Flensrocker.EventSourcing.Abstractions;
using Flensrocker.ToDo.Domain.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SuT = Flensrocker.ToDo.Abstractions.Tasks.TaskCommand;

namespace Flensrocker.ToDo.UnitTests.Tasks.TaskCommand;

[TestClass]
public sealed class StartHandlerTests : BaseTaskHandlerTests
{
  public StartHandlerTests(TestContext testContext)
    : base(testContext) { }

  [TestMethod]
  public async Task GivenOpenStateThenReturnStartedEvent()
  {
    var userId = Guid.CreateVersion7();
    var taskId = Guid.CreateVersion7();
    var taskVersion = 1;
    var taskCreatedAt = _timeProvider.GetUtcNow();
    var taskStartedAt = taskCreatedAt.AddMinutes(1.0);

    var testState = new TaskCommandState();
    testState.Apply(
      CreateTaskEvent(taskId, taskVersion++, new Abstractions.Tasks.TaskEvent.Opened { OpenedAt = taskCreatedAt, Title = "Some Title" })
    );

    await using var testScope = _serviceProvider.CreateAsyncScope();
    var testHandler = testScope.ServiceProvider.GetRequiredService<ICommandHandler<Abstractions.Tasks.TaskEvent, SuT, TaskCommandState>>();
    var testResult = await SuT
      .Start.Create(startedAt: taskStartedAt, byUserId: userId)
      .Pipe(async command => await testHandler.ExecuteCommandAsync(testState, command).ConfigureAwait(false))
      .ConfigureAwait(false);

    var result = Assert.IsInstanceOfType<DomainResult<Abstractions.Tasks.TaskEvent[]>.Success>(testResult);
    Assert.HasCount(1, result.Value);
    var startedEvent = Assert.IsInstanceOfType<Abstractions.Tasks.TaskEvent.Started>(result.Value[0]);
    Assert.AreEqual(taskStartedAt, startedEvent.StartedAt);
    Assert.AreEqual(userId, startedEvent.ByUserId);
  }

  [TestMethod]
  public async Task GivenReopenedStateThenReturnStartedEvent()
  {
    var userId = Guid.CreateVersion7();
    var taskId = Guid.CreateVersion7();
    var taskVersion = 1;
    var taskCreatedAt = _timeProvider.GetUtcNow();
    var taskStartedAt = taskCreatedAt.AddMinutes(1.0);

    var testState = new TaskCommandState();
    testState.Apply(
      CreateTaskEvent(taskId, taskVersion++, new Abstractions.Tasks.TaskEvent.Opened { OpenedAt = taskCreatedAt, Title = "Some Title" })
    );
    testState.Apply(CreateTaskEvent(taskId, taskVersion++, new Abstractions.Tasks.TaskEvent.Completed { }));
    testState.Apply(CreateTaskEvent(taskId, taskVersion++, new Abstractions.Tasks.TaskEvent.Reopened { }));

    await using var testScope = _serviceProvider.CreateAsyncScope();
    var testHandler = testScope.ServiceProvider.GetRequiredService<ICommandHandler<Abstractions.Tasks.TaskEvent, SuT, TaskCommandState>>();
    var testResult = await SuT
      .Start.Create(startedAt: taskStartedAt, byUserId: userId)
      .Pipe(async command => await testHandler.ExecuteCommandAsync(testState, command).ConfigureAwait(false))
      .ConfigureAwait(false);

    var result = Assert.IsInstanceOfType<DomainResult<Abstractions.Tasks.TaskEvent[]>.Success>(testResult);
    Assert.HasCount(1, result.Value);
    var startedEvent = Assert.IsInstanceOfType<Abstractions.Tasks.TaskEvent.Started>(result.Value[0]);
    Assert.AreEqual(taskStartedAt, startedEvent.StartedAt);
    Assert.AreEqual(userId, startedEvent.ByUserId);
  }

  [TestMethod]
  public async Task GivenPausedStateThenReturnStartedEvent()
  {
    var userId = Guid.CreateVersion7();
    var taskId = Guid.CreateVersion7();
    var taskVersion = 1;
    var taskCreatedAt = _timeProvider.GetUtcNow();
    var taskStartedAt = taskCreatedAt.AddMinutes(1.0);
    var taskPausedAt = taskStartedAt.AddMinutes(1.0);

    var testState = new TaskCommandState();
    testState.Apply(
      CreateTaskEvent(taskId, taskVersion++, new Abstractions.Tasks.TaskEvent.Opened { OpenedAt = taskCreatedAt, Title = "Some Title" })
    );
    testState.Apply(
      CreateTaskEvent(taskId, taskVersion++, new Abstractions.Tasks.TaskEvent.Started { StartedAt = taskStartedAt, ByUserId = userId })
    );
    testState.Apply(CreateTaskEvent(taskId, taskVersion++, new Abstractions.Tasks.TaskEvent.Paused { PausedAt = taskPausedAt }));

    await using var testScope = _serviceProvider.CreateAsyncScope();
    var testHandler = testScope.ServiceProvider.GetRequiredService<ICommandHandler<Abstractions.Tasks.TaskEvent, SuT, TaskCommandState>>();
    var testResult = await SuT
      .Start.Create(startedAt: taskStartedAt, byUserId: userId)
      .Pipe(async command => await testHandler.ExecuteCommandAsync(testState, command).ConfigureAwait(false))
      .ConfigureAwait(false);

    var result = Assert.IsInstanceOfType<DomainResult<Abstractions.Tasks.TaskEvent[]>.Success>(testResult);
    Assert.HasCount(1, result.Value);
    var startedEvent = Assert.IsInstanceOfType<Abstractions.Tasks.TaskEvent.Started>(result.Value[0]);
    Assert.AreEqual(taskStartedAt, startedEvent.StartedAt);
    Assert.AreEqual(userId, startedEvent.ByUserId);
  }

  [TestMethod]
  public async Task GivenStartedStateWithSameUserIdThenReturnEmptySuccess()
  {
    var userId = Guid.CreateVersion7();
    var taskId = Guid.CreateVersion7();
    var taskVersion = 1;
    var taskCreatedAt = _timeProvider.GetUtcNow();
    var taskStartedAt = taskCreatedAt.AddMinutes(1.0);

    var testState = new TaskCommandState();
    testState.Apply(
      CreateTaskEvent(taskId, taskVersion++, new Abstractions.Tasks.TaskEvent.Opened { OpenedAt = taskCreatedAt, Title = "Some Title" })
    );
    testState.Apply(
      CreateTaskEvent(taskId, taskVersion++, new Abstractions.Tasks.TaskEvent.Started { StartedAt = taskStartedAt, ByUserId = userId })
    );

    await using var testScope = _serviceProvider.CreateAsyncScope();
    var testHandler = testScope.ServiceProvider.GetRequiredService<ICommandHandler<Abstractions.Tasks.TaskEvent, SuT, TaskCommandState>>();
    var testResult = await SuT
      .Start.Create(startedAt: taskStartedAt, byUserId: userId)
      .Pipe(async command => await testHandler.ExecuteCommandAsync(testState, command).ConfigureAwait(false))
      .ConfigureAwait(false);

    var result = Assert.IsInstanceOfType<DomainResult<Abstractions.Tasks.TaskEvent[]>.Success>(testResult);
    Assert.IsEmpty(result.Value);
  }

  [TestMethod]
  public async Task GivenStartedStateWithDifferentUserIdThenReturnStartedEvent()
  {
    var userId = Guid.CreateVersion7();
    var otherUserId = Guid.CreateVersion7();
    var taskId = Guid.CreateVersion7();
    var taskVersion = 1;
    var taskCreatedAt = _timeProvider.GetUtcNow();
    var taskStartedAt = taskCreatedAt.AddMinutes(1.0);
    var taskOtherStartedAt = taskStartedAt.AddMinutes(1.0);

    var testState = new TaskCommandState();
    testState.Apply(
      CreateTaskEvent(taskId, taskVersion++, new Abstractions.Tasks.TaskEvent.Opened { OpenedAt = taskCreatedAt, Title = "Some Title" })
    );
    testState.Apply(
      CreateTaskEvent(taskId, taskVersion++, new Abstractions.Tasks.TaskEvent.Started { StartedAt = taskStartedAt, ByUserId = userId })
    );

    await using var testScope = _serviceProvider.CreateAsyncScope();
    var testHandler = testScope.ServiceProvider.GetRequiredService<ICommandHandler<Abstractions.Tasks.TaskEvent, SuT, TaskCommandState>>();
    var testResult = await SuT
      .Start.Create(startedAt: taskOtherStartedAt, byUserId: otherUserId)
      .Pipe(async command => await testHandler.ExecuteCommandAsync(testState, command).ConfigureAwait(false))
      .ConfigureAwait(false);

    var result = Assert.IsInstanceOfType<DomainResult<Abstractions.Tasks.TaskEvent[]>.Success>(testResult);
    Assert.HasCount(1, result.Value);
    var startedEvent = Assert.IsInstanceOfType<Abstractions.Tasks.TaskEvent.Started>(result.Value[0]);
    Assert.AreEqual(taskOtherStartedAt, startedEvent.StartedAt);
    Assert.AreEqual(otherUserId, startedEvent.ByUserId);
  }

  [TestMethod]
  public async Task GivenDismissedStateThenReturnBadRequest()
  {
    var userId = Guid.CreateVersion7();
    var taskId = Guid.CreateVersion7();
    var taskVersion = 1;
    var taskCreatedAt = _timeProvider.GetUtcNow();
    var taskStartedAt = taskCreatedAt.AddMinutes(1.0);

    var testState = new TaskCommandState();
    testState.Apply(
      CreateTaskEvent(taskId, taskVersion++, new Abstractions.Tasks.TaskEvent.Opened { OpenedAt = taskCreatedAt, Title = "Some Title" })
    );
    testState.Apply(CreateTaskEvent(taskId, taskVersion++, new Abstractions.Tasks.TaskEvent.Dismissed { Reason = "Some Reason" }));

    await using var testScope = _serviceProvider.CreateAsyncScope();
    var testHandler = testScope.ServiceProvider.GetRequiredService<ICommandHandler<Abstractions.Tasks.TaskEvent, SuT, TaskCommandState>>();
    var testResult = await SuT
      .Start.Create(startedAt: taskStartedAt, byUserId: userId)
      .Pipe(async command => await testHandler.ExecuteCommandAsync(testState, command).ConfigureAwait(false))
      .ConfigureAwait(false);

    Assert.IsInstanceOfType<DomainResult<Abstractions.Tasks.TaskEvent[]>.BadRequest>(testResult);
  }

  [TestMethod]
  public async Task GivenCompletedStateThenReturnBadRequest()
  {
    var userId = Guid.CreateVersion7();
    var taskId = Guid.CreateVersion7();
    var taskVersion = 1;
    var taskCreatedAt = _timeProvider.GetUtcNow();
    var taskStartedAt = taskCreatedAt.AddMinutes(1.0);

    var testState = new TaskCommandState();
    testState.Apply(
      CreateTaskEvent(taskId, taskVersion++, new Abstractions.Tasks.TaskEvent.Opened { OpenedAt = taskCreatedAt, Title = "Some Title" })
    );
    testState.Apply(CreateTaskEvent(taskId, taskVersion++, new Abstractions.Tasks.TaskEvent.Completed { }));

    await using var testScope = _serviceProvider.CreateAsyncScope();
    var testHandler = testScope.ServiceProvider.GetRequiredService<ICommandHandler<Abstractions.Tasks.TaskEvent, SuT, TaskCommandState>>();
    var testResult = await SuT
      .Start.Create(startedAt: taskStartedAt, byUserId: userId)
      .Pipe(async command => await testHandler.ExecuteCommandAsync(testState, command).ConfigureAwait(false))
      .ConfigureAwait(false);

    Assert.IsInstanceOfType<DomainResult<Abstractions.Tasks.TaskEvent[]>.BadRequest>(testResult);
  }

  [TestMethod]
  public async Task GivenEmptyStateThenReturnNotFound()
  {
    var userId = Guid.CreateVersion7();
    var taskStartedAt = _timeProvider.GetUtcNow();

    var testState = new TaskCommandState();

    await using var testScope = _serviceProvider.CreateAsyncScope();
    var testHandler = testScope.ServiceProvider.GetRequiredService<ICommandHandler<Abstractions.Tasks.TaskEvent, SuT, TaskCommandState>>();
    var testResult = await SuT
      .Start.Create(startedAt: taskStartedAt, byUserId: userId)
      .Pipe(async command => await testHandler.ExecuteCommandAsync(testState, command).ConfigureAwait(false))
      .ConfigureAwait(false);

    Assert.IsInstanceOfType<DomainResult<Abstractions.Tasks.TaskEvent[]>.NotFound>(testResult);
  }
}
