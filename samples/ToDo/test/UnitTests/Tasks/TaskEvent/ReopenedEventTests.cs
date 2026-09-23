using SuT = Flensrocker.ToDo.Abstractions.Tasks.TaskEvent;

namespace Flensrocker.ToDo.UnitTests.Tasks.TaskEvent;

[TestClass]
public sealed class ReopenedEventTests
{
  [TestMethod]
  public void IsJsonSerializable()
  {
    ToDoTestUtilities.AssertJsonSerializable<SuT, SuT.Reopened>(new SuT.Reopened { });
  }
}
