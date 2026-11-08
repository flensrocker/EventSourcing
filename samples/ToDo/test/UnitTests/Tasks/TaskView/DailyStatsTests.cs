using Flensrocker.EventSourcing.Test.Utilities;
using Microsoft.Extensions.Time.Testing;
using SuT = Flensrocker.ToDo.Abstractions.Tasks;

namespace Flensrocker.ToDo.UnitTests.Tasks.TaskView;

[TestClass]
public sealed class DailyStatsTests
{
  private readonly TimeProvider _timeProvider = new FakeTimeProvider();

  [TestMethod]
  [DataRow("20260930", "", "")]
  [DataRow("20260930", "f41abcd8-f1ab-4f90-a974-6c56e7f45815", "")]
  [DataRow("20260930", "", "ee7dd1da-7509-4646-8b92-9f5567748543")]
  [DataRow("20260930", "6965f1ce-ef67-4fa7-8fde-442c950f149a", "33a8f44a-b038-479b-89bd-266f3e9be286")]
  public void IdHasLength72(string dayData, string? tenantIdData, string? userIdData)
  {
    var day = DateOnly.ParseExact(dayData, "yyyyMMdd");
    var tenantId = string.IsNullOrWhiteSpace(tenantIdData) ? (Guid?)null : Guid.Parse(tenantIdData);
    var userId = string.IsNullOrWhiteSpace(userIdData) ? (Guid?)null : Guid.Parse(userIdData);

    var dailyStatsId = SuT.TaskView.DailyStats.GetKey(
      new EventSourcing.Abstractions.DomainEvent
      {
        Metadata = new EventSourcing.Abstractions.DomainEventMetadata
        {
          EventId = Guid.CreateVersion7(),
          StreamType = SuT.TaskConstants.TaskEvent.Base,
          StreamId = Guid.CreateVersion7(),
          StreamVersion = 1,
          TenantId = tenantId,
          CreatedAt = new DateTimeOffset(day, TimeOnly.FromTimeSpan(DateTime.UtcNow.TimeOfDay), TimeSpan.Zero),
          CreatedByUserId = userId,
        },
        Payload = new object(),
      }
    );

    Assert.AreEqual(72, dailyStatsId.Length);
  }

  [TestMethod]
  public void GivenTaskEventThenGetKeyReturnTaskId()
  {
    var tenantId = Guid.CreateVersion7();
    var userId = Guid.CreateVersion7();

    var taskId = Guid.CreateVersion7();
    var taskVersion = 1;
    var taskCreatedAt = _timeProvider.GetUtcNow();

    var openedPayload = new SuT.TaskEvent.Opened { OpenedAt = taskCreatedAt, Title = "Some Title" };
    var openedEvent = TaskTestUtilities.CreateTaskEvent(_timeProvider, tenantId, userId, taskId, taskVersion++, openedPayload);

    var key = TestUtilities.TestGetKey<SuT.TaskView.DailyStats>(openedEvent);
    var stringKey = Assert.IsInstanceOfType<string>(key);
    Assert.AreEqual(72, stringKey.Length);
  }

  [TestMethod]
  public void GivenCompletedEventThenReturnViewWithCountCompletedIncremented()
  {
    var tenantId = Guid.CreateVersion7();
    var userId = Guid.CreateVersion7();

    var taskId = Guid.CreateVersion7();
    var taskVersion = 1;

    var completedPayload = new SuT.TaskEvent.Completed { };
    var completedEvent = TaskTestUtilities.CreateTaskEvent(_timeProvider, tenantId, userId, taskId, taskVersion++, completedPayload);

    var testState = new SuT.TaskView.DailyStats();
    testState.Apply(completedEvent);

    (var completedDay, _, _) = completedEvent.Metadata.CreatedAt;
    Assert.IsTrue(TestUtilities.TestWhereKey(testState, completedEvent));
    Assert.AreEqual(completedDay, testState.Day);
    Assert.AreEqual(tenantId, testState.TenantId);
    Assert.AreEqual(userId, testState.UserId);
    Assert.AreEqual(0, testState.CountOpened);
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

    var dismissedPayload = new SuT.TaskEvent.Dismissed { Reason = "Some Reason" };
    var dismissedEvent = TaskTestUtilities.CreateTaskEvent(_timeProvider, tenantId, userId, taskId, taskVersion++, dismissedPayload);

    var testState = new SuT.TaskView.DailyStats();
    testState.Apply(dismissedEvent);

    (var dismissedDay, _, _) = dismissedEvent.Metadata.CreatedAt;
    Assert.IsTrue(TestUtilities.TestWhereKey(testState, dismissedEvent));
    Assert.AreEqual(dismissedDay, testState.Day);
    Assert.AreEqual(tenantId, testState.TenantId);
    Assert.AreEqual(userId, testState.UserId);
    Assert.AreEqual(0, testState.CountOpened);
    Assert.AreEqual(0, testState.CountCompleted);
    Assert.AreEqual(1, testState.CountDismissed);
  }

  [TestMethod]
  public void GivenOpenedEventThenReturnViewWithCountOpenedIncremented()
  {
    var tenantId = Guid.CreateVersion7();
    var userId = Guid.CreateVersion7();

    var taskId = Guid.CreateVersion7();
    var taskVersion = 1;
    var taskCreatedAt = _timeProvider.GetUtcNow();

    var openedPayload = new SuT.TaskEvent.Opened { OpenedAt = taskCreatedAt, Title = "Some Title" };
    var openedEvent = TaskTestUtilities.CreateTaskEvent(_timeProvider, tenantId, userId, taskId, taskVersion++, openedPayload);

    var testState = new SuT.TaskView.DailyStats();
    testState.Apply(openedEvent);

    (var openedDay, _, _) = openedEvent.Metadata.CreatedAt;
    Assert.IsTrue(TestUtilities.TestWhereKey(testState, openedEvent));
    Assert.AreEqual(openedDay, testState.Day);
    Assert.AreEqual(tenantId, testState.TenantId);
    Assert.AreEqual(userId, testState.UserId);
    Assert.AreEqual(1, testState.CountOpened);
    Assert.AreEqual(0, testState.CountCompleted);
    Assert.AreEqual(0, testState.CountDismissed);
  }

  [TestMethod]
  public void GivenReopenedEventThenReturnViewWithCountOpenedIncremented()
  {
    var tenantId = Guid.CreateVersion7();
    var userId = Guid.CreateVersion7();

    var taskId = Guid.CreateVersion7();
    var taskVersion = 1;

    var reopenedPayload = new SuT.TaskEvent.Reopened { };
    var reopenedEvent = TaskTestUtilities.CreateTaskEvent(_timeProvider, tenantId, userId, taskId, taskVersion++, reopenedPayload);

    var testState = new SuT.TaskView.DailyStats();
    testState.Apply(reopenedEvent);

    (var reopenedDay, _, _) = reopenedEvent.Metadata.CreatedAt;
    Assert.IsTrue(TestUtilities.TestWhereKey(testState, reopenedEvent));
    Assert.AreEqual(reopenedDay, testState.Day);
    Assert.AreEqual(tenantId, testState.TenantId);
    Assert.AreEqual(userId, testState.UserId);
    Assert.AreEqual(1, testState.CountOpened);
    Assert.AreEqual(0, testState.CountCompleted);
    Assert.AreEqual(0, testState.CountDismissed);
  }

  [TestMethod]
  public void GivenPausedEventThenReturnViewWithCountZero()
  {
    var tenantId = Guid.CreateVersion7();
    var userId = Guid.CreateVersion7();

    var taskId = Guid.CreateVersion7();
    var taskVersion = 1;
    var taskPausedAt = _timeProvider.GetUtcNow();

    var pausedPayload = new SuT.TaskEvent.Paused { PausedAt = taskPausedAt };
    var pausedEvent = TaskTestUtilities.CreateTaskEvent(_timeProvider, tenantId, userId, taskId, taskVersion++, pausedPayload);

    var testState = new SuT.TaskView.DailyStats();
    testState.Apply(pausedEvent);

    (var pausedDay, _, _) = pausedEvent.Metadata.CreatedAt;
    Assert.IsTrue(TestUtilities.TestWhereKey(testState, pausedEvent));
    Assert.AreEqual(pausedDay, testState.Day);
    Assert.AreEqual(tenantId, testState.TenantId);
    Assert.AreEqual(userId, testState.UserId);
    Assert.AreEqual(0, testState.CountOpened);
    Assert.AreEqual(0, testState.CountCompleted);
    Assert.AreEqual(0, testState.CountDismissed);
  }

  [TestMethod]
  public void GivenStartedEventThenReturnViewWithCountZero()
  {
    var tenantId = Guid.CreateVersion7();
    var userId = Guid.CreateVersion7();

    var taskId = Guid.CreateVersion7();
    var taskVersion = 1;
    var taskStartedAt = _timeProvider.GetUtcNow();

    var startedPayload = new SuT.TaskEvent.Started { StartedAt = taskStartedAt, ByUserId = userId };
    var startedEvent = TaskTestUtilities.CreateTaskEvent(_timeProvider, tenantId, userId, taskId, taskVersion++, startedPayload);

    var testState = new SuT.TaskView.DailyStats();
    testState.Apply(startedEvent);

    (var startedDay, _, _) = startedEvent.Metadata.CreatedAt;
    Assert.IsTrue(TestUtilities.TestWhereKey(testState, startedEvent));
    Assert.AreEqual(startedDay, testState.Day);
    Assert.AreEqual(tenantId, testState.TenantId);
    Assert.AreEqual(userId, testState.UserId);
    Assert.AreEqual(0, testState.CountOpened);
    Assert.AreEqual(0, testState.CountCompleted);
    Assert.AreEqual(0, testState.CountDismissed);
  }

  [TestMethod]
  public void GivenTitleChangedEventThenReturnViewWithCountZero()
  {
    var tenantId = Guid.CreateVersion7();
    var userId = Guid.CreateVersion7();

    var taskId = Guid.CreateVersion7();
    var taskVersion = 1;

    var titleChangedPayload = new SuT.TaskEvent.TitleChanged { Title = "Some Title" };
    var titleChangedEvent = TaskTestUtilities.CreateTaskEvent(_timeProvider, tenantId, userId, taskId, taskVersion++, titleChangedPayload);

    var testState = new SuT.TaskView.DailyStats();
    testState.Apply(titleChangedEvent);

    (var startedDay, _, _) = titleChangedEvent.Metadata.CreatedAt;
    Assert.IsTrue(TestUtilities.TestWhereKey(testState, titleChangedEvent));
    Assert.AreEqual(startedDay, testState.Day);
    Assert.AreEqual(tenantId, testState.TenantId);
    Assert.AreEqual(userId, testState.UserId);
    Assert.AreEqual(0, testState.CountOpened);
    Assert.AreEqual(0, testState.CountCompleted);
    Assert.AreEqual(0, testState.CountDismissed);
  }
}
