using Flensrocker.ToDo.Abstractions.Tasks;

namespace Flensrocker.ToDo.UnitTests.Tasks.TaskQuery;

[TestClass]
public sealed class DailyStatsQueryTests
{
  [TestMethod]
  [DataRow(null, null, null, null)]
  [DataRow("8d9368ce-1710-4170-8c19-89ea90841d43", null, null, null)]
  [DataRow(null, "8753cb45-c18b-4f59-acfe-79bfedd71a35", null, null)]
  [DataRow("8d9368ce-1710-4170-8c19-89ea90841d43", "8753cb45-c18b-4f59-acfe-79bfedd71a35", null, null)]
  [DataRow(null, null, "2026-09-20", null)]
  [DataRow("8d9368ce-1710-4170-8c19-89ea90841d43", null, "2026-09-20", null)]
  [DataRow(null, "8753cb45-c18b-4f59-acfe-79bfedd71a35", "2026-09-20", null)]
  [DataRow("8d9368ce-1710-4170-8c19-89ea90841d43", "8753cb45-c18b-4f59-acfe-79bfedd71a35", "2026-09-20", null)]
  [DataRow(null, null, null, "2026-09-27")]
  [DataRow("8d9368ce-1710-4170-8c19-89ea90841d43", null, null, "2026-09-27")]
  [DataRow(null, "8753cb45-c18b-4f59-acfe-79bfedd71a35", null, "2026-09-27")]
  [DataRow("8d9368ce-1710-4170-8c19-89ea90841d43", "8753cb45-c18b-4f59-acfe-79bfedd71a35", null, "2026-09-27")]
  [DataRow(null, null, "2026-09-20", "2026-09-27")]
  [DataRow("8d9368ce-1710-4170-8c19-89ea90841d43", null, "2026-09-20", "2026-09-27")]
  [DataRow(null, "8753cb45-c18b-4f59-acfe-79bfedd71a35", "2026-09-20", "2026-09-27")]
  [DataRow("8d9368ce-1710-4170-8c19-89ea90841d43", "8753cb45-c18b-4f59-acfe-79bfedd71a35", "2026-09-20", "2026-09-27")]
  public void GivenValidInputsThenReturnSuccess(string? tenantId, string? userId, string? fromDate, string? toDate)
  {
    var validTenantId = string.IsNullOrWhiteSpace(tenantId) ? (Guid?)null : Guid.Parse(tenantId);
    var validUserId = string.IsNullOrWhiteSpace(userId) ? (Guid?)null : Guid.Parse(userId);
    var validFromDate = string.IsNullOrWhiteSpace(fromDate) ? (DateOnly?)null : DateOnly.Parse(fromDate);
    var validToDate = string.IsNullOrWhiteSpace(toDate) ? (DateOnly?)null : DateOnly.Parse(toDate);

    var testResult = TaskQueryRequest.DailyStats.Create(
      tenantId: validTenantId,
      userId: validUserId,
      fromDate: validFromDate,
      toDate: validToDate
    );

    var result = Assert.IsInstanceOfType<DomainResult<TaskQueryRequest.DailyStats>.Success>(testResult);
    Assert.AreEqual(validTenantId, result.Value.TenantId);
    Assert.AreEqual(validUserId, result.Value.UserId);
    Assert.AreEqual(validFromDate, result.Value.FromDate);
    Assert.AreEqual(validToDate, result.Value.ToDate);
  }

  [TestMethod]
  [DataRow("2026-09-28", "2026-09-27")]
  public void GivenInvalidInputsThenReturnValidationProblem(string? fromDate, string? toDate)
  {
    var invalidFromDate = string.IsNullOrWhiteSpace(fromDate) ? (DateOnly?)null : DateOnly.Parse(fromDate);
    var invalidToDate = string.IsNullOrWhiteSpace(toDate) ? (DateOnly?)null : DateOnly.Parse(toDate);

    var testResult = TaskQueryRequest.DailyStats.Create(tenantId: null, userId: null, fromDate: invalidFromDate, toDate: invalidToDate);

    var result = Assert.IsInstanceOfType<DomainResult<TaskQueryRequest.DailyStats>.ValidationProblem>(testResult);
    Assert.IsTrue(result.Errors.ContainsKey(nameof(TaskQueryRequest.DailyStats.FromDate)));
    Assert.IsTrue(result.Errors.ContainsKey(nameof(TaskQueryRequest.DailyStats.ToDate)));
  }

  [TestMethod]
  public void GivenNoInputsThenReturnEmptyResponse()
  {
    var testResponse = new TaskQueryResponse.DailyStats { ItemList = [] };

    Assert.IsEmpty(testResponse.ItemList);
  }

  [TestMethod]
  public void IsJsonSerializable()
  {
    ToDoTestUtilities.AssertJsonSerializable(
      TaskQueryRequest.DailyStats.Create(
        tenantId: Guid.CreateVersion7(),
        userId: Guid.CreateVersion7(),
        fromDate: DateOnly.FromDateTime(DateTime.Today),
        toDate: DateOnly.FromDateTime(DateTime.Today)
      )
    );

    ToDoTestUtilities.AssertJsonSerializable<TaskQueryResponse.DailyStats>(
      new TaskQueryResponse.DailyStats
      {
        ItemList =
        [
          new()
          {
            Day = DateOnly.FromDateTime(DateTime.Today),
            UserId = Guid.CreateVersion7(),
            CountNoted = 1,
            CountCompleted = 2,
            CountDismissed = 3,
          },
        ],
      }
    );
  }
}
