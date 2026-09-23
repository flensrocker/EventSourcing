using SuT = Flensrocker.ToDo.Abstractions.Tasks.TaskCommand;

namespace Flensrocker.ToDo.UnitTests.Tasks.TaskCommand;

[TestClass]
public sealed class ReopenCommandTests
{
  [TestMethod]
  public void GivenValidInputsThenReturnSuccess()
  {
    var testResult = SuT.Reopen.Create();

    Assert.IsInstanceOfType<DomainResult<SuT.Reopen>.Success>(testResult);
  }

  [TestMethod]
  public void IsJsonSerializable()
  {
    ToDoTestUtilities.AssertJsonSerializable<SuT, SuT.Reopen>(SuT.Reopen.Create());
  }
}
