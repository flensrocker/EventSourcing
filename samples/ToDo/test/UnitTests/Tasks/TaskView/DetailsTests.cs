using Microsoft.Extensions.Time.Testing;
using SuT = Flensrocker.ToDo.Abstractions.Tasks.TaskView;

namespace Flensrocker.ToDo.UnitTests.Tasks.TaskView;

[TestClass]
public sealed class DetailsTests
{
  private readonly TimeProvider _timeProvider = new FakeTimeProvider();

  [TestMethod]
  public void GivenNotedEventThenReturnViewWithOpenedStatus()
  {
    var tenantId = Guid.CreateVersion7();
    var userId = Guid.CreateVersion7();

    var taskId = Guid.CreateVersion7();
    var taskVersion = 1;
    var taskCreatedAt = _timeProvider.GetUtcNow();

    var notedPayload = new Abstractions.Tasks.TaskEvent.Noted { CreatedAt = taskCreatedAt, Title = "Some Title" };
    var notedEvent = TaskTestUtilities.CreateTaskEvent(_timeProvider, tenantId, userId, taskId, taskVersion++, notedPayload);

    var testState = new SuT.Details();
    testState.Apply(notedEvent);

    Assert.AreEqual(taskId, testState.TaskId);
    Assert.AreEqual(taskVersion - 1, testState.Version);
    Assert.AreEqual(notedPayload.Title, testState.Title);
    Assert.AreEqual(Abstractions.Tasks.TaskStatus.Open, testState.Status);
    Assert.IsNull(testState.DismissReason);
    Assert.IsNull(testState.StartedByUserId);
  }

  [TestMethod]
  public void GivenNotedAndStartedEventThenReturnViewWithStartedStatus()
  {
    var tenantId = Guid.CreateVersion7();
    var userId = Guid.CreateVersion7();

    var taskId = Guid.CreateVersion7();
    var taskVersion = 1;
    var taskCreatedAt = _timeProvider.GetUtcNow();
    var taskStartedAt = taskCreatedAt.AddMinutes(1.0);

    var notedPayload = new Abstractions.Tasks.TaskEvent.Noted { CreatedAt = taskCreatedAt, Title = "Some Title" };
    var notedEvent = TaskTestUtilities.CreateTaskEvent(_timeProvider, tenantId, userId, taskId, taskVersion++, notedPayload);
    var startedPayload = new Abstractions.Tasks.TaskEvent.Started { StartedAt = taskStartedAt, ByUserId = userId };
    var startedEvent = TaskTestUtilities.CreateTaskEvent(_timeProvider, tenantId, userId, taskId, taskVersion++, startedPayload);

    var testState = new SuT.Details();
    testState.Apply(notedEvent);
    testState.Apply(startedEvent);

    Assert.AreEqual(taskId, testState.TaskId);
    Assert.AreEqual(taskVersion - 1, testState.Version);
    Assert.AreEqual(notedPayload.Title, testState.Title);
    Assert.AreEqual(Abstractions.Tasks.TaskStatus.Started, testState.Status);
    Assert.IsNull(testState.DismissReason);
    Assert.AreEqual(userId, testState.StartedByUserId);
  }

  [TestMethod]
  public void GivenNotedStartedAndPausedEventThenReturnViewWithPausedStatus()
  {
    var tenantId = Guid.CreateVersion7();
    var userId = Guid.CreateVersion7();

    var taskId = Guid.CreateVersion7();
    var taskVersion = 1;
    var taskCreatedAt = _timeProvider.GetUtcNow();
    var taskStartedAt = taskCreatedAt.AddMinutes(1.0);
    var taskPausedAt = taskStartedAt.AddMinutes(1.0);

    var notedPayload = new Abstractions.Tasks.TaskEvent.Noted { CreatedAt = taskCreatedAt, Title = "Some Title" };
    var notedEvent = TaskTestUtilities.CreateTaskEvent(_timeProvider, tenantId, userId, taskId, taskVersion++, notedPayload);
    var startedPayload = new Abstractions.Tasks.TaskEvent.Started { StartedAt = taskStartedAt, ByUserId = userId };
    var startedEvent = TaskTestUtilities.CreateTaskEvent(_timeProvider, tenantId, userId, taskId, taskVersion++, startedPayload);
    var pausedPayload = new Abstractions.Tasks.TaskEvent.Paused { PausedAt = taskPausedAt };
    var pausedEvent = TaskTestUtilities.CreateTaskEvent(_timeProvider, tenantId, userId, taskId, taskVersion++, pausedPayload);

    var testState = new SuT.Details();
    testState.Apply(notedEvent);
    testState.Apply(startedEvent);
    testState.Apply(pausedEvent);

    Assert.AreEqual(taskId, testState.TaskId);
    Assert.AreEqual(taskVersion - 1, testState.Version);
    Assert.AreEqual(notedPayload.Title, testState.Title);
    Assert.AreEqual(Abstractions.Tasks.TaskStatus.Paused, testState.Status);
    Assert.IsNull(testState.DismissReason);
    Assert.IsNull(testState.StartedByUserId);
  }

  [TestMethod]
  public void GivenNotedAndCompletedEventThenReturnViewWithCompletedStatus()
  {
    var tenantId = Guid.CreateVersion7();
    var userId = Guid.CreateVersion7();

    var taskId = Guid.CreateVersion7();
    var taskVersion = 1;
    var taskCreatedAt = _timeProvider.GetUtcNow();

    var notedPayload = new Abstractions.Tasks.TaskEvent.Noted { CreatedAt = taskCreatedAt, Title = "Some Title" };
    var notedEvent = TaskTestUtilities.CreateTaskEvent(_timeProvider, tenantId, userId, taskId, taskVersion++, notedPayload);
    var completedPayload = new Abstractions.Tasks.TaskEvent.Completed { };
    var completedEvent = TaskTestUtilities.CreateTaskEvent(_timeProvider, tenantId, userId, taskId, taskVersion++, completedPayload);

    var testState = new SuT.Details();
    testState.Apply(notedEvent);
    testState.Apply(completedEvent);

    Assert.AreEqual(taskId, testState.TaskId);
    Assert.AreEqual(taskVersion - 1, testState.Version);
    Assert.AreEqual(notedPayload.Title, testState.Title);
    Assert.AreEqual(Abstractions.Tasks.TaskStatus.Completed, testState.Status);
    Assert.IsNull(testState.DismissReason);
    Assert.IsNull(testState.StartedByUserId);
  }

  [TestMethod]
  public void GivenNotedAndDismissedEventThenReturnViewWithDismissedStatus()
  {
    var tenantId = Guid.CreateVersion7();
    var userId = Guid.CreateVersion7();

    var taskId = Guid.CreateVersion7();
    var taskVersion = 1;
    var taskCreatedAt = _timeProvider.GetUtcNow();

    var notedPayload = new Abstractions.Tasks.TaskEvent.Noted { CreatedAt = taskCreatedAt, Title = "Some Title" };
    var notedEvent = TaskTestUtilities.CreateTaskEvent(_timeProvider, tenantId, userId, taskId, taskVersion++, notedPayload);
    var dismissedPayload = new Abstractions.Tasks.TaskEvent.Dismissed { Reason = "Some Reason" };
    var dismissedEvent = TaskTestUtilities.CreateTaskEvent(_timeProvider, tenantId, userId, taskId, taskVersion++, dismissedPayload);

    var testState = new SuT.Details();
    testState.Apply(notedEvent);
    testState.Apply(dismissedEvent);

    Assert.AreEqual(taskId, testState.TaskId);
    Assert.AreEqual(taskVersion - 1, testState.Version);
    Assert.AreEqual(notedPayload.Title, testState.Title);
    Assert.AreEqual(Abstractions.Tasks.TaskStatus.Dismissed, testState.Status);
    Assert.AreEqual(dismissedPayload.Reason, testState.DismissReason);
    Assert.IsNull(testState.StartedByUserId);
  }

  [TestMethod]
  public void GivenNotedCompletedAndReopenedEventThenReturnViewWithOpenStatus()
  {
    var tenantId = Guid.CreateVersion7();
    var userId = Guid.CreateVersion7();

    var taskId = Guid.CreateVersion7();
    var taskVersion = 1;
    var taskCreatedAt = _timeProvider.GetUtcNow();

    var notedPayload = new Abstractions.Tasks.TaskEvent.Noted { CreatedAt = taskCreatedAt, Title = "Some Title" };
    var notedEvent = TaskTestUtilities.CreateTaskEvent(_timeProvider, tenantId, userId, taskId, taskVersion++, notedPayload);
    var completedPayload = new Abstractions.Tasks.TaskEvent.Completed { };
    var completedEvent = TaskTestUtilities.CreateTaskEvent(_timeProvider, tenantId, userId, taskId, taskVersion++, completedPayload);
    var reopenedPayload = new Abstractions.Tasks.TaskEvent.Reopened { };
    var reopenedEvent = TaskTestUtilities.CreateTaskEvent(_timeProvider, tenantId, userId, taskId, taskVersion++, reopenedPayload);

    var testState = new SuT.Details();
    testState.Apply(notedEvent);
    testState.Apply(completedEvent);
    testState.Apply(reopenedEvent);

    Assert.AreEqual(taskId, testState.TaskId);
    Assert.AreEqual(taskVersion - 1, testState.Version);
    Assert.AreEqual(notedPayload.Title, testState.Title);
    Assert.AreEqual(Abstractions.Tasks.TaskStatus.Open, testState.Status);
    Assert.IsNull(testState.DismissReason);
    Assert.IsNull(testState.StartedByUserId);
  }

  [TestMethod]
  public void GivenNotedDismissedAndReopenedEventThenReturnViewWithOpenStatus()
  {
    var tenantId = Guid.CreateVersion7();
    var userId = Guid.CreateVersion7();

    var taskId = Guid.CreateVersion7();
    var taskVersion = 1;
    var taskCreatedAt = _timeProvider.GetUtcNow();

    var notedPayload = new Abstractions.Tasks.TaskEvent.Noted { CreatedAt = taskCreatedAt, Title = "Some Title" };
    var notedEvent = TaskTestUtilities.CreateTaskEvent(_timeProvider, tenantId, userId, taskId, taskVersion++, notedPayload);
    var dismissedPayload = new Abstractions.Tasks.TaskEvent.Dismissed { Reason = "Some Reason" };
    var dismissedEvent = TaskTestUtilities.CreateTaskEvent(_timeProvider, tenantId, userId, taskId, taskVersion++, dismissedPayload);
    var reopenedPayload = new Abstractions.Tasks.TaskEvent.Reopened { };
    var reopenedEvent = TaskTestUtilities.CreateTaskEvent(_timeProvider, tenantId, userId, taskId, taskVersion++, reopenedPayload);

    var testState = new SuT.Details();
    testState.Apply(notedEvent);
    testState.Apply(dismissedEvent);
    testState.Apply(reopenedEvent);

    Assert.AreEqual(taskId, testState.TaskId);
    Assert.AreEqual(taskVersion - 1, testState.Version);
    Assert.AreEqual(notedPayload.Title, testState.Title);
    Assert.AreEqual(Abstractions.Tasks.TaskStatus.Open, testState.Status);
    Assert.IsNull(testState.DismissReason);
    Assert.IsNull(testState.StartedByUserId);
  }

  [TestMethod]
  public void IsJsonSerializable()
  {
    var tenantId = Guid.CreateVersion7();
    var userId = Guid.CreateVersion7();

    var taskId = Guid.CreateVersion7();
    var taskVersion = 1;
    var taskCreatedAt = _timeProvider.GetUtcNow();

    var notedPayload = new Abstractions.Tasks.TaskEvent.Noted { CreatedAt = taskCreatedAt, Title = "Some Title" };
    var notedEvent = TaskTestUtilities.CreateTaskEvent(_timeProvider, tenantId, userId, taskId, taskVersion++, notedPayload);
    var dismissedPayload = new Abstractions.Tasks.TaskEvent.Dismissed { Reason = "Some Reason" };
    var dismissedEvent = TaskTestUtilities.CreateTaskEvent(_timeProvider, tenantId, userId, taskId, taskVersion++, dismissedPayload);

    var testState = new SuT.Details();
    testState.Apply(notedEvent);
    testState.Apply(dismissedEvent);

    ToDoTestUtilities.AssertJsonSerializable<SuT.Details>(testState);
  }
}
