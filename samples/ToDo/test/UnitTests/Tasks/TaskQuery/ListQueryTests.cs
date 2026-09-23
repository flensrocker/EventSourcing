using Flensrocker.ToDo.Abstractions.Tasks;

namespace Flensrocker.ToDo.UnitTests.Tasks.TaskQuery;

[TestClass]
public sealed class ListQueryTests
{
  [TestMethod]
  [DataRow(null)]
  [DataRow("6baf2b4c-ae0a-415b-83fe-0b8b055e041c")]
  public void GivenValidInputsThenReturnSuccess(string? tenantId)
  {
    var validTenantId = string.IsNullOrWhiteSpace(tenantId) ? (Guid?)null : Guid.Parse(tenantId);
    var testResult = TaskQueryRequest.List.Create(tenantId: validTenantId);

    var result = Assert.IsInstanceOfType<DomainResult<TaskQueryRequest.List>.Success>(testResult);
    Assert.AreEqual(validTenantId, result.Value.TenantId);
  }
}
