using SuT = Flensrocker.ToDo.Abstractions.Tasks.TaskEvent;

namespace Flensrocker.ToDo.UnitTests.Tasks.TaskEvent;

[TestClass]
public sealed class NotedEventTests
{
  [TestMethod]
  public void IsJsonSerializable()
  {
    ToDoTestUtilities.AssertJsonSerializable<SuT, SuT.Noted>(new SuT.Noted { Title = "Some Title", CreatedAt = DateTimeOffset.UtcNow });
  }
}
