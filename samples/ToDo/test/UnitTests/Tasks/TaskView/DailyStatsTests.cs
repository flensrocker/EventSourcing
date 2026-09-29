using Microsoft.Extensions.Time.Testing;
using SuT = Flensrocker.ToDo.Abstractions.Tasks.TaskView;

namespace Flensrocker.ToDo.UnitTests.Tasks.TaskView;

[TestClass]
public sealed class DailyStatsTests
{
  private readonly TimeProvider _timeProvider = new FakeTimeProvider();

  [TestMethod]
  public void GivenCompletedEventThenReturnViewWithCountCompletedIncremented()
  {
    var tenantId = Guid.CreateVersion7();
    var userId = Guid.CreateVersion7();

    var taskId = Guid.CreateVersion7();
    var taskVersion = 1;

    var completedPayload = new Abstractions.Tasks.TaskEvent.Completed { };
    var completedEvent = TaskTestUtilities.CreateTaskEvent(_timeProvider, tenantId, userId, taskId, taskVersion++, completedPayload);

    var testState = new SuT.DailyStats();
    testState.Apply(completedEvent);

    (var notedDay, _, _) = completedEvent.Metadata.CreatedAt;
    Assert.AreEqual(notedDay, testState.Day);
    Assert.AreEqual(tenantId, testState.TenantId);
    Assert.AreEqual(userId, testState.UserId);
    Assert.AreEqual(0, testState.CountNoted);
    Assert.AreEqual(1, testState.CountCompleted);
    Assert.AreEqual(0, testState.CountDismissed);
  }

  [TestMethod]
  public void GivenDismissedEventThenReturnViewWithCountDismissedIncremented()
  {
    var tenantId = Guid.CreateVersion7();
    var userId = Guid.CreateVersion7();

    var taskId = Guid.CreateVersion7();
    var taskVersion = 1;

    var dismissedPayload = new Abstractions.Tasks.TaskEvent.Dismissed { Reason = "Some Reason" };
    var dismissedEvent = TaskTestUtilities.CreateTaskEvent(_timeProvider, tenantId, userId, taskId, taskVersion++, dismissedPayload);

    var testState = new SuT.DailyStats();
    testState.Apply(dismissedEvent);

    (var notedDay, _, _) = dismissedEvent.Metadata.CreatedAt;
    Assert.AreEqual(notedDay, testState.Day);
    Assert.AreEqual(tenantId, testState.TenantId);
    Assert.AreEqual(userId, testState.UserId);
    Assert.AreEqual(0, testState.CountNoted);
    Assert.AreEqual(0, testState.CountCompleted);
    Assert.AreEqual(1, testState.CountDismissed);
  }

  [TestMethod]
  public void GivenNotedEventThenReturnViewWithCountNotedIncremented()
  {
    var tenantId = Guid.CreateVersion7();
    var userId = Guid.CreateVersion7();

    var taskId = Guid.CreateVersion7();
    var taskVersion = 1;
    var taskCreatedAt = _timeProvider.GetUtcNow();

    var notedPayload = new Abstractions.Tasks.TaskEvent.Noted { CreatedAt = taskCreatedAt, Title = "Some Title" };
    var notedEvent = TaskTestUtilities.CreateTaskEvent(_timeProvider, tenantId, userId, taskId, taskVersion++, notedPayload);

    var testState = new SuT.DailyStats();
    testState.Apply(notedEvent);

    (var notedDay, _, _) = notedEvent.Metadata.CreatedAt;
    Assert.AreEqual(notedDay, testState.Day);
    Assert.AreEqual(tenantId, testState.TenantId);
    Assert.AreEqual(userId, testState.UserId);
    Assert.AreEqual(1, testState.CountNoted);
    Assert.AreEqual(0, testState.CountCompleted);
    Assert.AreEqual(0, testState.CountDismissed);
  }

  [TestMethod]
  public void GivenReopenedEventThenReturnViewWithCountNotedIncremented()
  {
    var tenantId = Guid.CreateVersion7();
    var userId = Guid.CreateVersion7();

    var taskId = Guid.CreateVersion7();
    var taskVersion = 1;

    var reopenedPayload = new Abstractions.Tasks.TaskEvent.Reopened { };
    var reopenedEvent = TaskTestUtilities.CreateTaskEvent(_timeProvider, tenantId, userId, taskId, taskVersion++, reopenedPayload);

    var testState = new SuT.DailyStats();
    testState.Apply(reopenedEvent);

    (var notedDay, _, _) = reopenedEvent.Metadata.CreatedAt;
    Assert.AreEqual(notedDay, testState.Day);
    Assert.AreEqual(tenantId, testState.TenantId);
    Assert.AreEqual(userId, testState.UserId);
    Assert.AreEqual(1, testState.CountNoted);
    Assert.AreEqual(0, testState.CountCompleted);
    Assert.AreEqual(0, testState.CountDismissed);
  }
}
