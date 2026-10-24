using SuT = Flensrocker.ToDo.Abstractions.Tasks.TaskEvent;

namespace Flensrocker.ToDo.UnitTests.Tasks.TaskEvent;

[TestClass]
public sealed class PausedEventTests
{
  [TestMethod]
  public void IsJsonSerializable()
  {
    ToDoTestUtilities.AssertJsonSerializable<SuT, SuT.Paused>(new SuT.Paused { PausedAt = DateTimeOffset.UtcNow });
  }
}
