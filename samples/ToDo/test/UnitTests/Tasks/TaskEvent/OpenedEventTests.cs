using SuT = Flensrocker.ToDo.Abstractions.Tasks.TaskEvent;

namespace Flensrocker.ToDo.UnitTests.Tasks.TaskEvent;

[TestClass]
public sealed class OpenedEventTests
{
  [TestMethod]
  public void IsJsonSerializable()
  {
    ToDoTestUtilities.AssertJsonSerializable<SuT, SuT.Opened>(new SuT.Opened { Title = "Some Title", OpenedAt = DateTimeOffset.UtcNow });
  }
}
