using Flensrocker.EventSourcing.Abstractions;
using Flensrocker.ToDo.Domain.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SuT = Flensrocker.ToDo.Abstractions.Tasks.TaskCommand;

namespace Flensrocker.ToDo.UnitTests.Tasks.TaskCommand;

[TestClass]
public sealed class DismissHandlerTests : BaseTaskHandlerTests
{
  private readonly string _validReason = "Valid Reason";

  public DismissHandlerTests(TestContext testContext)
    : base(testContext) { }

  [TestMethod]
  public async Task GivenOpenStateThenReturnDismissedEvent()
  {
    var taskId = Guid.CreateVersion7();
    var taskVersion = 1;
    var taskCreatedAt = _timeProvider.GetUtcNow();

    var testState = new TaskCommandState();
    testState.Apply(
      CreateTaskEvent(taskId, taskVersion++, new Abstractions.Tasks.TaskEvent.Noted { CreatedAt = taskCreatedAt, Title = "Some Title" })
    );

    await using var testScope = _serviceProvider.CreateAsyncScope();
    var testHandler = testScope.ServiceProvider.GetRequiredService<ICommandHandler<Abstractions.Tasks.TaskEvent, SuT, TaskCommandState>>();
    var testResult = await SuT
      .Dismiss.Create(reason: _validReason)
      .Pipe(async command => await testHandler.ExecuteCommandAsync(testState, command).ConfigureAwait(false))
      .ConfigureAwait(false);

    var result = Assert.IsInstanceOfType<DomainResult<Abstractions.Tasks.TaskEvent[]>.Success>(testResult);
    Assert.HasCount(1, result.Value);
    var dismissedEvent = Assert.IsInstanceOfType<Abstractions.Tasks.TaskEvent.Dismissed>(result.Value[0]);
    Assert.AreEqual(_validReason, dismissedEvent.Reason);
  }

  [TestMethod]
  public async Task GivenPausedStateThenReturnDismissedEvent()
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
      .Dismiss.Create(reason: _validReason)
      .Pipe(async command => await testHandler.ExecuteCommandAsync(testState, command).ConfigureAwait(false))
      .ConfigureAwait(false);

    var result = Assert.IsInstanceOfType<DomainResult<Abstractions.Tasks.TaskEvent[]>.Success>(testResult);
    Assert.HasCount(1, result.Value);
    var dismissedEvent = Assert.IsInstanceOfType<Abstractions.Tasks.TaskEvent.Dismissed>(result.Value[0]);
    Assert.AreEqual(_validReason, dismissedEvent.Reason);
  }

  [TestMethod]
  public async Task GivenStartedStateThenReturnDismissedEvent()
  {
    var userId = Guid.CreateVersion7();
    var taskId = Guid.CreateVersion7();
    var taskVersion = 1;
    var taskCreatedAt = _timeProvider.GetUtcNow();
    var taskStartedAt = taskCreatedAt.AddMinutes(1.0);

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
      .Dismiss.Create(reason: _validReason)
      .Pipe(async command => await testHandler.ExecuteCommandAsync(testState, command).ConfigureAwait(false))
      .ConfigureAwait(false);

    var result = Assert.IsInstanceOfType<DomainResult<Abstractions.Tasks.TaskEvent[]>.Success>(testResult);
    Assert.HasCount(1, result.Value);
    var dismissedEvent = Assert.IsInstanceOfType<Abstractions.Tasks.TaskEvent.Dismissed>(result.Value[0]);
    Assert.AreEqual(_validReason, dismissedEvent.Reason);
  }

  [TestMethod]
  public async Task GivenDismissedStateWithSameReasonThenReturnEmptySuccess()
  {
    var taskId = Guid.CreateVersion7();
    var taskVersion = 1;
    var taskCreatedAt = _timeProvider.GetUtcNow();

    var testState = new TaskCommandState();
    testState.Apply(
      CreateTaskEvent(taskId, taskVersion++, new Abstractions.Tasks.TaskEvent.Noted { CreatedAt = taskCreatedAt, Title = "Some Title" })
    );
    testState.Apply(CreateTaskEvent(taskId, taskVersion++, new Abstractions.Tasks.TaskEvent.Dismissed { Reason = _validReason }));

    await using var testScope = _serviceProvider.CreateAsyncScope();
    var testHandler = testScope.ServiceProvider.GetRequiredService<ICommandHandler<Abstractions.Tasks.TaskEvent, SuT, TaskCommandState>>();
    var testResult = await SuT
      .Dismiss.Create(reason: _validReason)
      .Pipe(async command => await testHandler.ExecuteCommandAsync(testState, command).ConfigureAwait(false))
      .ConfigureAwait(false);

    var result = Assert.IsInstanceOfType<DomainResult<Abstractions.Tasks.TaskEvent[]>.Success>(testResult);
    Assert.IsEmpty(result.Value);
  }

  [TestMethod]
  public async Task GivenDismissedStateWithDifferentReasonThenReturnBadRequest()
  {
    var taskId = Guid.CreateVersion7();
    var taskVersion = 1;
    var taskCreatedAt = _timeProvider.GetUtcNow();

    var testState = new TaskCommandState();
    testState.Apply(
      CreateTaskEvent(taskId, taskVersion++, new Abstractions.Tasks.TaskEvent.Noted { CreatedAt = taskCreatedAt, Title = "Some Title" })
    );
    testState.Apply(
      CreateTaskEvent(taskId, taskVersion++, new Abstractions.Tasks.TaskEvent.Dismissed { Reason = "Some different reason" })
    );

    await using var testScope = _serviceProvider.CreateAsyncScope();
    var testHandler = testScope.ServiceProvider.GetRequiredService<ICommandHandler<Abstractions.Tasks.TaskEvent, SuT, TaskCommandState>>();
    var testResult = await SuT
      .Dismiss.Create(reason: _validReason)
      .Pipe(async command => await testHandler.ExecuteCommandAsync(testState, command).ConfigureAwait(false))
      .ConfigureAwait(false);

    Assert.IsInstanceOfType<DomainResult<Abstractions.Tasks.TaskEvent[]>.BadRequest>(testResult);
  }

  [TestMethod]
  public async Task GivenEmptyStateThenReturnNotFound()
  {
    var testState = new TaskCommandState();

    await using var testScope = _serviceProvider.CreateAsyncScope();
    var testHandler = testScope.ServiceProvider.GetRequiredService<ICommandHandler<Abstractions.Tasks.TaskEvent, SuT, TaskCommandState>>();
    var testResult = await SuT
      .Dismiss.Create(reason: _validReason)
      .Pipe(async command => await testHandler.ExecuteCommandAsync(testState, command).ConfigureAwait(false))
      .ConfigureAwait(false);

    Assert.IsInstanceOfType<DomainResult<Abstractions.Tasks.TaskEvent[]>.NotFound>(testResult);
  }

  [TestMethod]
  public async Task GivenCompletedStateThenReturnBadRequest()
  {
    var taskId = Guid.CreateVersion7();
    var taskVersion = 1;
    var taskCreatedAt = _timeProvider.GetUtcNow();

    var testState = new TaskCommandState();
    testState.Apply(
      CreateTaskEvent(taskId, taskVersion++, new Abstractions.Tasks.TaskEvent.Noted { CreatedAt = taskCreatedAt, Title = "Some Title" })
    );
    testState.Apply(CreateTaskEvent(taskId, taskVersion++, new Abstractions.Tasks.TaskEvent.Completed { }));

    await using var testScope = _serviceProvider.CreateAsyncScope();
    var testHandler = testScope.ServiceProvider.GetRequiredService<ICommandHandler<Abstractions.Tasks.TaskEvent, SuT, TaskCommandState>>();
    var testResult = await SuT
      .Dismiss.Create(reason: _validReason)
      .Pipe(async command => await testHandler.ExecuteCommandAsync(testState, command).ConfigureAwait(false))
      .ConfigureAwait(false);

    Assert.IsInstanceOfType<DomainResult<Abstractions.Tasks.TaskEvent[]>.BadRequest>(testResult);
  }
}
