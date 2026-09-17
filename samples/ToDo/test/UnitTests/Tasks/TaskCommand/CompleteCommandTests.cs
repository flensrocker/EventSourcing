using SuT = Flensrocker.ToDo.Abstractions.Tasks.TaskCommand;

namespace Flensrocker.ToDo.UnitTests.Tasks.TaskCommand;

[TestClass]
public sealed class CompleteCommandTests
{
  [TestMethod]
  public void GivenValidInputsThenReturnSuccess()
  {
    var testResult = SuT.Complete.Create();

    Assert.IsInstanceOfType<DomainResult<SuT.Complete>.Success>(testResult);
  }
}
