using SuT = Flensrocker.ToDo.Abstractions.Tasks.TaskCommand;

namespace Flensrocker.ToDo.UnitTests.Tasks.TaskCommand;

[TestClass]
public sealed class PauseCommandTests
{
  [TestMethod]
  [DataRow(null)]
  [DataRow("")]
  [DataRow("2026-10-24T12:34:56Z")]
  public void GivenValidInputsThenReturnSuccess(string? pausedAt)
  {
    var validPausedAt = string.IsNullOrWhiteSpace(pausedAt) ? (DateTimeOffset?)null : DateTimeOffset.Parse(pausedAt);

    var testResult = SuT.Pause.Create(pausedAt: validPausedAt);

    Assert.IsInstanceOfType<DomainResult<SuT.Pause>.Success>(testResult);
  }

  [TestMethod]
  public void IsJsonSerializable()
  {
    ToDoTestUtilities.AssertJsonSerializable<SuT, SuT.Pause>(SuT.Pause.Create(pausedAt: DateTimeOffset.UtcNow));
  }
}
