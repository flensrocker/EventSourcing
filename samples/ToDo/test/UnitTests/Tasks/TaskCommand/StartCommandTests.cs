using SuT = Flensrocker.ToDo.Abstractions.Tasks.TaskCommand;

namespace Flensrocker.ToDo.UnitTests.Tasks.TaskCommand;

[TestClass]
public sealed class StartCommandTests
{
  [TestMethod]
  [DataRow(null, null)]
  [DataRow("", "")]
  [DataRow("2026-10-24T12:34:56Z", "a4d455ab-4868-4bfb-a5f2-fa6c23e473de")]
  public void GivenValidInputsThenReturnSuccess(string? startedAt, string? byUserId)
  {
    var validStartedAt = string.IsNullOrWhiteSpace(startedAt) ? (DateTimeOffset?)null : DateTimeOffset.Parse(startedAt);
    var validByUserId = string.IsNullOrWhiteSpace(byUserId) ? (Guid?)null : Guid.Parse(byUserId);

    var testResult = SuT.Start.Create(startedAt: validStartedAt, byUserId: validByUserId);

    Assert.IsInstanceOfType<DomainResult<SuT.Start>.Success>(testResult);
  }

  [TestMethod]
  public void IsJsonSerializable()
  {
    ToDoTestUtilities.AssertJsonSerializable<SuT, SuT.Start>(SuT.Start.Create(startedAt: DateTimeOffset.UtcNow, byUserId: Guid.CreateVersion7()));
  }
}
