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

    var notedPayload = new Abstractions.Tasks.TaskEvent.Noted { CreatedAt = taskCreatedAt, Title = "Some Title" };
    var notedEvent = TaskTestUtilities.CreateTaskEvent(_timeProvider, tenantId, userId, taskId, taskVersion++, notedPayload);

    var key = ToDoTestUtilities.TestGetKey<SuT.ListItem>(notedEvent);
    var guidKey = Assert.IsInstanceOfType<Guid>(key);
    Assert.AreEqual(taskId, guidKey);
  }

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

    var testState = new SuT.ListItem();
    testState.Apply(notedEvent);

    Assert.IsTrue(ToDoTestUtilities.TestWhereKey(testState, notedEvent));
    Assert.AreEqual(taskId, testState.TaskId);
    Assert.AreEqual(taskVersion - 1, testState.Version);
    Assert.AreEqual(tenantId, testState.TenantId);
    Assert.AreEqual(notedPayload.CreatedAt, testState.CreatedAt);
    Assert.AreEqual(notedPayload.Title, testState.Title);
    Assert.AreEqual(Abstractions.Tasks.TaskStatus.Open, testState.Status);
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

    var testState = new SuT.ListItem();
    testState.Apply(notedEvent);
    testState.Apply(completedEvent);

    Assert.IsTrue(ToDoTestUtilities.TestWhereKey(testState, notedEvent));
    Assert.AreEqual(taskId, testState.TaskId);
    Assert.AreEqual(taskVersion - 1, testState.Version);
    Assert.AreEqual(tenantId, testState.TenantId);
    Assert.AreEqual(notedPayload.CreatedAt, testState.CreatedAt);
    Assert.AreEqual(notedPayload.Title, testState.Title);
    Assert.AreEqual(Abstractions.Tasks.TaskStatus.Completed, testState.Status);
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

    var testState = new SuT.ListItem();
    testState.Apply(notedEvent);
    testState.Apply(dismissedEvent);

    Assert.IsTrue(ToDoTestUtilities.TestWhereKey(testState, notedEvent));
    Assert.AreEqual(taskId, testState.TaskId);
    Assert.AreEqual(taskVersion - 1, testState.Version);
    Assert.AreEqual(tenantId, testState.TenantId);
    Assert.AreEqual(notedPayload.CreatedAt, testState.CreatedAt);
    Assert.AreEqual(notedPayload.Title, testState.Title);
    Assert.AreEqual(Abstractions.Tasks.TaskStatus.Dismissed, testState.Status);
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

    var testState = new SuT.ListItem();
    testState.Apply(notedEvent);
    testState.Apply(completedEvent);
    testState.Apply(reopenedEvent);

    Assert.IsTrue(ToDoTestUtilities.TestWhereKey(testState, notedEvent));
    Assert.AreEqual(taskId, testState.TaskId);
    Assert.AreEqual(taskVersion - 1, testState.Version);
    Assert.AreEqual(tenantId, testState.TenantId);
    Assert.AreEqual(notedPayload.CreatedAt, testState.CreatedAt);
    Assert.AreEqual(notedPayload.Title, testState.Title);
    Assert.AreEqual(Abstractions.Tasks.TaskStatus.Open, testState.Status);
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

    var testState = new SuT.ListItem();
    testState.Apply(notedEvent);
    testState.Apply(dismissedEvent);
    testState.Apply(reopenedEvent);

    Assert.IsTrue(ToDoTestUtilities.TestWhereKey(testState, notedEvent));
    Assert.AreEqual(taskId, testState.TaskId);
    Assert.AreEqual(taskVersion - 1, testState.Version);
    Assert.AreEqual(tenantId, testState.TenantId);
    Assert.AreEqual(notedPayload.CreatedAt, testState.CreatedAt);
    Assert.AreEqual(notedPayload.Title, testState.Title);
    Assert.AreEqual(Abstractions.Tasks.TaskStatus.Open, testState.Status);
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

    var testState = new SuT.ListItem();
    testState.Apply(notedEvent);
    testState.Apply(dismissedEvent);

    ToDoTestUtilities.AssertJsonSerializable<SuT.ListItem>(testState);
  }
}
