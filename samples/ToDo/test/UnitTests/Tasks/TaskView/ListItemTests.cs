using Flensrocker.EventSourcing.Test.Utilities;
using Microsoft.Extensions.Time.Testing;
using SuT = Flensrocker.ToDo.Abstractions.Tasks.TaskView;

namespace Flensrocker.ToDo.UnitTests.Tasks.TaskView;

[TestClass]
public sealed class ListItemTests
{
  private readonly TimeProvider _timeProvider = new FakeTimeProvider();

  [TestMethod]
  public void GivenTaskEventThenGetKeyReturnTaskId()
  {
    var tenantId = Guid.CreateVersion7();
    var userId = Guid.CreateVersion7();

    var taskId = Guid.CreateVersion7();
    var taskVersion = 1;
    var taskCreatedAt = _timeProvider.GetUtcNow();

    var openedPayload = new Abstractions.Tasks.TaskEvent.Opened { OpenedAt = taskCreatedAt, Title = "Some Title" };
    var openedEvent = TaskTestUtilities.CreateTaskEvent(_timeProvider, tenantId, userId, taskId, taskVersion++, openedPayload);

    var key = TestUtilities.TestGetKey<SuT.ListItem>(openedEvent);
    var guidKey = Assert.IsInstanceOfType<Guid>(key);
    Assert.AreEqual(taskId, guidKey);
  }

  [TestMethod]
  public void GivenOpenedEventThenReturnViewWithOpenedStatus()
  {
    var tenantId = Guid.CreateVersion7();
    var userId = Guid.CreateVersion7();

    var taskId = Guid.CreateVersion7();
    var taskVersion = 1;
    var taskCreatedAt = _timeProvider.GetUtcNow();

    var openedPayload = new Abstractions.Tasks.TaskEvent.Opened { OpenedAt = taskCreatedAt, Title = "Some Title" };
    var openedEvent = TaskTestUtilities.CreateTaskEvent(_timeProvider, tenantId, userId, taskId, taskVersion++, openedPayload);

    var testState = new SuT.ListItem();
    testState.Apply(openedEvent);

    Assert.IsTrue(TestUtilities.TestWhereKey(testState, openedEvent));
    Assert.AreEqual(taskId, testState.TaskId);
    Assert.AreEqual(taskVersion - 1, testState.Version);
    Assert.AreEqual(tenantId, testState.TenantId);
    Assert.AreEqual(openedPayload.OpenedAt, testState.OpenedAt);
    Assert.AreEqual(openedPayload.Title, testState.Title);
    Assert.AreEqual(Abstractions.Tasks.TaskStatus.Opened, testState.Status);
  }

  [TestMethod]
  public void GivenOpenedAndTitleChangedEventThenReturnViewWithOpenedStatusAndChangedTitle()
  {
    var tenantId = Guid.CreateVersion7();
    var userId = Guid.CreateVersion7();

    var taskId = Guid.CreateVersion7();
    var taskVersion = 1;
    var taskCreatedAt = _timeProvider.GetUtcNow();

    var openedPayload = new Abstractions.Tasks.TaskEvent.Opened { OpenedAt = taskCreatedAt, Title = "Some Title" };
    var openedEvent = TaskTestUtilities.CreateTaskEvent(_timeProvider, tenantId, userId, taskId, taskVersion++, openedPayload);
    var titleChangedPayload = new Abstractions.Tasks.TaskEvent.TitleChanged { Title = "Cahnged Title" };
    var titleChangedEvent = TaskTestUtilities.CreateTaskEvent(_timeProvider, tenantId, userId, taskId, taskVersion++, titleChangedPayload);

    var testState = new SuT.ListItem();
    testState.Apply(openedEvent);
    testState.Apply(titleChangedEvent);

    Assert.IsTrue(TestUtilities.TestWhereKey(testState, openedEvent));
    Assert.AreEqual(taskId, testState.TaskId);
    Assert.AreEqual(taskVersion - 1, testState.Version);
    Assert.AreEqual(tenantId, testState.TenantId);
    Assert.AreEqual(openedPayload.OpenedAt, testState.OpenedAt);
    Assert.AreEqual(titleChangedPayload.Title, testState.Title);
    Assert.AreEqual(Abstractions.Tasks.TaskStatus.Opened, testState.Status);
  }

  [TestMethod]
  public void GivenOpenedAndStartedEventThenReturnViewWithStartedStatus()
  {
    var tenantId = Guid.CreateVersion7();
    var userId = Guid.CreateVersion7();

    var taskId = Guid.CreateVersion7();
    var taskVersion = 1;
    var taskCreatedAt = _timeProvider.GetUtcNow();
    var taskStartedAt = taskCreatedAt.AddMinutes(1.0);

    var openedPayload = new Abstractions.Tasks.TaskEvent.Opened { OpenedAt = taskCreatedAt, Title = "Some Title" };
    var openedEvent = TaskTestUtilities.CreateTaskEvent(_timeProvider, tenantId, userId, taskId, taskVersion++, openedPayload);
    var startedPayload = new Abstractions.Tasks.TaskEvent.Started { StartedAt = taskStartedAt, ByUserId = userId };
    var startedEvent = TaskTestUtilities.CreateTaskEvent(_timeProvider, tenantId, userId, taskId, taskVersion++, startedPayload);

    var testState = new SuT.ListItem();
    testState.Apply(openedEvent);
    testState.Apply(startedEvent);

    Assert.IsTrue(TestUtilities.TestWhereKey(testState, openedEvent));
    Assert.AreEqual(taskId, testState.TaskId);
    Assert.AreEqual(taskVersion - 1, testState.Version);
    Assert.AreEqual(tenantId, testState.TenantId);
    Assert.AreEqual(openedPayload.OpenedAt, testState.OpenedAt);
    Assert.AreEqual(openedPayload.Title, testState.Title);
    Assert.AreEqual(Abstractions.Tasks.TaskStatus.Started, testState.Status);
  }

  [TestMethod]
  public void GivenOpenedStartedAndPausedEventThenReturnViewWithPausedStatus()
  {
    var tenantId = Guid.CreateVersion7();
    var userId = Guid.CreateVersion7();

    var taskId = Guid.CreateVersion7();
    var taskVersion = 1;
    var taskCreatedAt = _timeProvider.GetUtcNow();
    var taskStartedAt = taskCreatedAt.AddMinutes(1.0);
    var taskPausedAt = taskStartedAt.AddMinutes(1.0);

    var openedPayload = new Abstractions.Tasks.TaskEvent.Opened { OpenedAt = taskCreatedAt, Title = "Some Title" };
    var openedEvent = TaskTestUtilities.CreateTaskEvent(_timeProvider, tenantId, userId, taskId, taskVersion++, openedPayload);
    var startedPayload = new Abstractions.Tasks.TaskEvent.Started { StartedAt = taskStartedAt, ByUserId = userId };
    var startedEvent = TaskTestUtilities.CreateTaskEvent(_timeProvider, tenantId, userId, taskId, taskVersion++, startedPayload);
    var pausedPayload = new Abstractions.Tasks.TaskEvent.Paused { PausedAt = taskPausedAt };
    var pausedEvent = TaskTestUtilities.CreateTaskEvent(_timeProvider, tenantId, userId, taskId, taskVersion++, pausedPayload);

    var testState = new SuT.ListItem();
    testState.Apply(openedEvent);
    testState.Apply(startedEvent);
    testState.Apply(pausedEvent);

    Assert.IsTrue(TestUtilities.TestWhereKey(testState, openedEvent));
    Assert.AreEqual(taskId, testState.TaskId);
    Assert.AreEqual(taskVersion - 1, testState.Version);
    Assert.AreEqual(tenantId, testState.TenantId);
    Assert.AreEqual(openedPayload.OpenedAt, testState.OpenedAt);
    Assert.AreEqual(openedPayload.Title, testState.Title);
    Assert.AreEqual(Abstractions.Tasks.TaskStatus.Paused, testState.Status);
  }

  [TestMethod]
  public void GivenOpenedAndCompletedEventThenReturnViewWithCompletedStatus()
  {
    var tenantId = Guid.CreateVersion7();
    var userId = Guid.CreateVersion7();

    var taskId = Guid.CreateVersion7();
    var taskVersion = 1;
    var taskCreatedAt = _timeProvider.GetUtcNow();

    var openedPayload = new Abstractions.Tasks.TaskEvent.Opened { OpenedAt = taskCreatedAt, Title = "Some Title" };
    var openedEvent = TaskTestUtilities.CreateTaskEvent(_timeProvider, tenantId, userId, taskId, taskVersion++, openedPayload);
    var completedPayload = new Abstractions.Tasks.TaskEvent.Completed { };
    var completedEvent = TaskTestUtilities.CreateTaskEvent(_timeProvider, tenantId, userId, taskId, taskVersion++, completedPayload);

    var testState = new SuT.ListItem();
    testState.Apply(openedEvent);
    testState.Apply(completedEvent);

    Assert.IsTrue(TestUtilities.TestWhereKey(testState, openedEvent));
    Assert.AreEqual(taskId, testState.TaskId);
    Assert.AreEqual(taskVersion - 1, testState.Version);
    Assert.AreEqual(tenantId, testState.TenantId);
    Assert.AreEqual(openedPayload.OpenedAt, testState.OpenedAt);
    Assert.AreEqual(openedPayload.Title, testState.Title);
    Assert.AreEqual(Abstractions.Tasks.TaskStatus.Completed, testState.Status);
  }

  [TestMethod]
  public void GivenOpenedAndDismissedEventThenReturnViewWithDismissedStatus()
  {
    var tenantId = Guid.CreateVersion7();
    var userId = Guid.CreateVersion7();

    var taskId = Guid.CreateVersion7();
    var taskVersion = 1;
    var taskCreatedAt = _timeProvider.GetUtcNow();

    var openedPayload = new Abstractions.Tasks.TaskEvent.Opened { OpenedAt = taskCreatedAt, Title = "Some Title" };
    var openedEvent = TaskTestUtilities.CreateTaskEvent(_timeProvider, tenantId, userId, taskId, taskVersion++, openedPayload);
    var dismissedPayload = new Abstractions.Tasks.TaskEvent.Dismissed { Reason = "Some Reason" };
    var dismissedEvent = TaskTestUtilities.CreateTaskEvent(_timeProvider, tenantId, userId, taskId, taskVersion++, dismissedPayload);

    var testState = new SuT.ListItem();
    testState.Apply(openedEvent);
    testState.Apply(dismissedEvent);

    Assert.IsTrue(TestUtilities.TestWhereKey(testState, openedEvent));
    Assert.AreEqual(taskId, testState.TaskId);
    Assert.AreEqual(taskVersion - 1, testState.Version);
    Assert.AreEqual(tenantId, testState.TenantId);
    Assert.AreEqual(openedPayload.OpenedAt, testState.OpenedAt);
    Assert.AreEqual(openedPayload.Title, testState.Title);
    Assert.AreEqual(Abstractions.Tasks.TaskStatus.Dismissed, testState.Status);
  }

  [TestMethod]
  public void GivenOpenedCompletedAndReopenedEventThenReturnViewWithOpenStatus()
  {
    var tenantId = Guid.CreateVersion7();
    var userId = Guid.CreateVersion7();

    var taskId = Guid.CreateVersion7();
    var taskVersion = 1;
    var taskCreatedAt = _timeProvider.GetUtcNow();

    var openedPayload = new Abstractions.Tasks.TaskEvent.Opened { OpenedAt = taskCreatedAt, Title = "Some Title" };
    var openedEvent = TaskTestUtilities.CreateTaskEvent(_timeProvider, tenantId, userId, taskId, taskVersion++, openedPayload);
    var completedPayload = new Abstractions.Tasks.TaskEvent.Completed { };
    var completedEvent = TaskTestUtilities.CreateTaskEvent(_timeProvider, tenantId, userId, taskId, taskVersion++, completedPayload);
    var reopenedPayload = new Abstractions.Tasks.TaskEvent.Reopened { };
    var reopenedEvent = TaskTestUtilities.CreateTaskEvent(_timeProvider, tenantId, userId, taskId, taskVersion++, reopenedPayload);

    var testState = new SuT.ListItem();
    testState.Apply(openedEvent);
    testState.Apply(completedEvent);
    testState.Apply(reopenedEvent);

    Assert.IsTrue(TestUtilities.TestWhereKey(testState, openedEvent));
    Assert.AreEqual(taskId, testState.TaskId);
    Assert.AreEqual(taskVersion - 1, testState.Version);
    Assert.AreEqual(tenantId, testState.TenantId);
    Assert.AreEqual(openedPayload.OpenedAt, testState.OpenedAt);
    Assert.AreEqual(openedPayload.Title, testState.Title);
    Assert.AreEqual(Abstractions.Tasks.TaskStatus.Opened, testState.Status);
  }

  [TestMethod]
  public void GivenOpenedDismissedAndReopenedEventThenReturnViewWithOpenStatus()
  {
    var tenantId = Guid.CreateVersion7();
    var userId = Guid.CreateVersion7();

    var taskId = Guid.CreateVersion7();
    var taskVersion = 1;
    var taskCreatedAt = _timeProvider.GetUtcNow();

    var openedPayload = new Abstractions.Tasks.TaskEvent.Opened { OpenedAt = taskCreatedAt, Title = "Some Title" };
    var openedEvent = TaskTestUtilities.CreateTaskEvent(_timeProvider, tenantId, userId, taskId, taskVersion++, openedPayload);
    var dismissedPayload = new Abstractions.Tasks.TaskEvent.Dismissed { Reason = "Some Reason" };
    var dismissedEvent = TaskTestUtilities.CreateTaskEvent(_timeProvider, tenantId, userId, taskId, taskVersion++, dismissedPayload);
    var reopenedPayload = new Abstractions.Tasks.TaskEvent.Reopened { };
    var reopenedEvent = TaskTestUtilities.CreateTaskEvent(_timeProvider, tenantId, userId, taskId, taskVersion++, reopenedPayload);

    var testState = new SuT.ListItem();
    testState.Apply(openedEvent);
    testState.Apply(dismissedEvent);
    testState.Apply(reopenedEvent);

    Assert.IsTrue(TestUtilities.TestWhereKey(testState, openedEvent));
    Assert.AreEqual(taskId, testState.TaskId);
    Assert.AreEqual(taskVersion - 1, testState.Version);
    Assert.AreEqual(tenantId, testState.TenantId);
    Assert.AreEqual(openedPayload.OpenedAt, testState.OpenedAt);
    Assert.AreEqual(openedPayload.Title, testState.Title);
    Assert.AreEqual(Abstractions.Tasks.TaskStatus.Opened, testState.Status);
  }

  [TestMethod]
  public void IsJsonSerializable()
  {
    var tenantId = Guid.CreateVersion7();
    var userId = Guid.CreateVersion7();

    var taskId = Guid.CreateVersion7();
    var taskVersion = 1;
    var taskCreatedAt = _timeProvider.GetUtcNow();

    var openedPayload = new Abstractions.Tasks.TaskEvent.Opened { OpenedAt = taskCreatedAt, Title = "Some Title" };
    var openedEvent = TaskTestUtilities.CreateTaskEvent(_timeProvider, tenantId, userId, taskId, taskVersion++, openedPayload);
    var dismissedPayload = new Abstractions.Tasks.TaskEvent.Dismissed { Reason = "Some Reason" };
    var dismissedEvent = TaskTestUtilities.CreateTaskEvent(_timeProvider, tenantId, userId, taskId, taskVersion++, dismissedPayload);

    var testState = new SuT.ListItem();
    testState.Apply(openedEvent);
    testState.Apply(dismissedEvent);

    ToDoTestUtilities.AssertJsonSerializable<SuT.ListItem>(testState);
  }
}
