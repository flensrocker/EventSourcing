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

    (var notedDay, _, _) = completedEvent.Metadata.CreatedAt;
    Assert.IsTrue(ToDoTestUtilities.TestWhereKey(testState, completedEvent));
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

    var dismissedPayload = new SuT.TaskEvent.Dismissed { Reason = "Some Reason" };
    var dismissedEvent = TaskTestUtilities.CreateTaskEvent(_timeProvider, tenantId, userId, taskId, taskVersion++, dismissedPayload);

    var testState = new SuT.TaskView.DailyStats();
    testState.Apply(dismissedEvent);

    (var notedDay, _, _) = dismissedEvent.Metadata.CreatedAt;
    Assert.IsTrue(ToDoTestUtilities.TestWhereKey(testState, dismissedEvent));
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

    var notedPayload = new SuT.TaskEvent.Noted { CreatedAt = taskCreatedAt, Title = "Some Title" };
    var notedEvent = TaskTestUtilities.CreateTaskEvent(_timeProvider, tenantId, userId, taskId, taskVersion++, notedPayload);

    var testState = new SuT.TaskView.DailyStats();
    testState.Apply(notedEvent);

    (var notedDay, _, _) = notedEvent.Metadata.CreatedAt;
    Assert.IsTrue(ToDoTestUtilities.TestWhereKey(testState, notedEvent));
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

    var reopenedPayload = new SuT.TaskEvent.Reopened { };
    var reopenedEvent = TaskTestUtilities.CreateTaskEvent(_timeProvider, tenantId, userId, taskId, taskVersion++, reopenedPayload);

    var testState = new SuT.TaskView.DailyStats();
    testState.Apply(reopenedEvent);

    (var notedDay, _, _) = reopenedEvent.Metadata.CreatedAt;
    Assert.IsTrue(ToDoTestUtilities.TestWhereKey(testState, reopenedEvent));
    Assert.AreEqual(notedDay, testState.Day);
    Assert.AreEqual(tenantId, testState.TenantId);
    Assert.AreEqual(userId, testState.UserId);
    Assert.AreEqual(1, testState.CountNoted);
    Assert.AreEqual(0, testState.CountCompleted);
    Assert.AreEqual(0, testState.CountDismissed);
  }
}
