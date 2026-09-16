using SuT = Flensrocker.ToDo.Abstractions.Tasks.TaskCommand;

namespace Flensrocker.ToDo.UnitTests.Tasks.TaskCommand;

[TestClass]
public sealed class CompleteTests
{
  [TestMethod]
  public void ValidInputsReturnSuccess()
  {
    var testResult = SuT.Complete.Create();

    _ = Assert.IsInstanceOfType<DomainResult<SuT.Complete>.Success>(testResult);
  }
}
