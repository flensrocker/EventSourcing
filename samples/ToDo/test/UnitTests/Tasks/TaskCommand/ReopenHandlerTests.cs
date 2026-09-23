using Flensrocker.EventSourcing.Abstractions;
using Flensrocker.ToDo.Abstractions.Tasks;
using Flensrocker.ToDo.Domain.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SuT = Flensrocker.ToDo.Abstractions.Tasks.TaskCommand;

namespace Flensrocker.ToDo.UnitTests.Tasks.TaskCommand;

[TestClass]
public sealed class ReopenHandlerTests : BaseTaskHandlerTests
{
  [TestMethod]
  public async Task GivenCompletedStateThenReturnReopenedEvent()
  {
    var taskId = Guid.CreateVersion7();
    var taskVersion = 1;
    var taskCreatedAt = _timeProvider.GetUtcNow();

    var testState = new TaskCommandState();
    testState.Apply(CreateTaskEvent(taskId, taskVersion++, new TaskEvent.Noted { CreatedAt = taskCreatedAt, Title = "Some Title" }));
    testState.Apply(CreateTaskEvent(taskId, taskVersion++, new TaskEvent.Completed { }));

    await using var testScope = _serviceProvider.CreateAsyncScope();
    var testHandler = testScope.ServiceProvider.GetRequiredService<ICommandHandler<TaskEvent, SuT, TaskCommandState>>();
    var testResult = await SuT.Reopen.Create().Pipe(async command => await testHandler.ExecuteCommandAsync(testState, command));

    var result = Assert.IsInstanceOfType<DomainResult<TaskEvent[]>.Success>(testResult);
    Assert.HasCount(1, result.Value);
    Assert.IsInstanceOfType<TaskEvent.Reopened>(result.Value[0]);
  }

  [TestMethod]
  public async Task GivenDismissedStateThenReturnReopenedEvent()
  {
    var taskId = Guid.CreateVersion7();
    var taskVersion = 1;
    var taskCreatedAt = _timeProvider.GetUtcNow();

    var testState = new TaskCommandState();
    testState.Apply(CreateTaskEvent(taskId, taskVersion++, new TaskEvent.Noted { CreatedAt = taskCreatedAt, Title = "Some Title" }));
    testState.Apply(CreateTaskEvent(taskId, taskVersion++, new TaskEvent.Dismissed { Reason = "Some Reason" }));

    await using var testScope = _serviceProvider.CreateAsyncScope();
    var testHandler = testScope.ServiceProvider.GetRequiredService<ICommandHandler<TaskEvent, SuT, TaskCommandState>>();
    var testResult = await SuT.Reopen.Create().Pipe(async command => await testHandler.ExecuteCommandAsync(testState, command));

    var result = Assert.IsInstanceOfType<DomainResult<TaskEvent[]>.Success>(testResult);
    Assert.HasCount(1, result.Value);
    Assert.IsInstanceOfType<TaskEvent.Reopened>(result.Value[0]);
  }

  [TestMethod]
  public async Task GivenOpenedStateThenReturnEmptySuccess()
  {
    var taskId = Guid.CreateVersion7();
    var taskVersion = 1;
    var taskCreatedAt = _timeProvider.GetUtcNow();

    var testState = new TaskCommandState();
    testState.Apply(CreateTaskEvent(taskId, taskVersion++, new TaskEvent.Noted { CreatedAt = taskCreatedAt, Title = "Some Title" }));

    await using var testScope = _serviceProvider.CreateAsyncScope();
    var testHandler = testScope.ServiceProvider.GetRequiredService<ICommandHandler<TaskEvent, SuT, TaskCommandState>>();
    var testResult = await SuT.Reopen.Create().Pipe(async command => await testHandler.ExecuteCommandAsync(testState, command));

    var result = Assert.IsInstanceOfType<DomainResult<TaskEvent[]>.Success>(testResult);
    Assert.IsEmpty(result.Value);
  }

  [TestMethod]
  public async Task GivenReopenedStateThenReturnEmptySuccess()
  {
    var taskId = Guid.CreateVersion7();
    var taskVersion = 1;
    var taskCreatedAt = _timeProvider.GetUtcNow();

    var testState = new TaskCommandState();
    testState.Apply(CreateTaskEvent(taskId, taskVersion++, new TaskEvent.Noted { CreatedAt = taskCreatedAt, Title = "Some Title" }));
    testState.Apply(CreateTaskEvent(taskId, taskVersion++, new TaskEvent.Completed { }));
    testState.Apply(CreateTaskEvent(taskId, taskVersion++, new TaskEvent.Reopened { }));

    await using var testScope = _serviceProvider.CreateAsyncScope();
    var testHandler = testScope.ServiceProvider.GetRequiredService<ICommandHandler<TaskEvent, SuT, TaskCommandState>>();
    var testResult = await SuT.Reopen.Create().Pipe(async command => await testHandler.ExecuteCommandAsync(testState, command));

    var result = Assert.IsInstanceOfType<DomainResult<TaskEvent[]>.Success>(testResult);
    Assert.IsEmpty(result.Value);
  }

  [TestMethod]
  public async Task GivenEmptyStateThenReturnNotFound()
  {
    var testState = new TaskCommandState();

    await using var testScope = _serviceProvider.CreateAsyncScope();
    var testHandler = testScope.ServiceProvider.GetRequiredService<ICommandHandler<TaskEvent, SuT, TaskCommandState>>();
    var testResult = await SuT.Reopen.Create().Pipe(async command => await testHandler.ExecuteCommandAsync(testState, command));

    Assert.IsInstanceOfType<DomainResult<TaskEvent[]>.NotFound>(testResult);
  }
}
