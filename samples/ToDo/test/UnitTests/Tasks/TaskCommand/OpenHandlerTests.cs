using Flensrocker.EventSourcing.Abstractions;
using Flensrocker.ToDo.Domain.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SuT = Flensrocker.ToDo.Abstractions.Tasks.TaskCommand;

namespace Flensrocker.ToDo.UnitTests.Tasks.TaskCommand;

[TestClass]
public sealed class OpenHandlerTests : BaseTaskHandlerTests
{
  private readonly string _validTitle = "Valid Title";

  public OpenHandlerTests(TestContext testContext)
    : base(testContext) { }

  [TestMethod]
  public async Task GivenEmptyStateThenReturnOpenedEvent()
  {
    var testState = new TaskCommandState();

    await using var testScope = _serviceProvider.CreateAsyncScope();
    var testHandler = testScope.ServiceProvider.GetRequiredService<ICommandHandler<Abstractions.Tasks.TaskEvent, SuT, TaskCommandState>>();
    var testResult = await SuT
      .Open.Create(title: _validTitle)
      .Pipe(async command => await testHandler.ExecuteCommandAsync(testState, command).ConfigureAwait(false))
      .ConfigureAwait(false);

    var result = Assert.IsInstanceOfType<DomainResult<Abstractions.Tasks.TaskEvent[]>.Success>(testResult);
    Assert.HasCount(1, result.Value);
    var openedEvent = Assert.IsInstanceOfType<Abstractions.Tasks.TaskEvent.Opened>(result.Value[0]);
    Assert.AreEqual(_timeProvider.GetUtcNow(), openedEvent.OpenedAt);
    Assert.AreEqual(_validTitle, openedEvent.Title);
  }

  [TestMethod]
  public async Task GivenNonEmptyStateThenReturnBadRequest()
  {
    var taskId = Guid.CreateVersion7();
    var taskVersion = 1;
    var taskCreatedAt = _timeProvider.GetUtcNow();

    var testState = new TaskCommandState();
    testState.Apply(
      CreateTaskEvent(taskId, taskVersion++, new Abstractions.Tasks.TaskEvent.Opened { OpenedAt = taskCreatedAt, Title = _validTitle })
    );

    await using var testScope = _serviceProvider.CreateAsyncScope();
    var testHandler = testScope.ServiceProvider.GetRequiredService<ICommandHandler<Abstractions.Tasks.TaskEvent, SuT, TaskCommandState>>();
    var testResult = await SuT
      .Open.Create(title: _validTitle)
      .Pipe(async command => await testHandler.ExecuteCommandAsync(testState, command).ConfigureAwait(false))
      .ConfigureAwait(false);

    Assert.IsInstanceOfType<DomainResult<Abstractions.Tasks.TaskEvent[]>.BadRequest>(testResult);
  }
}
