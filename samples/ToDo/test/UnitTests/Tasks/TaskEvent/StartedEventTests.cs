using SuT = Flensrocker.ToDo.Abstractions.Tasks.TaskEvent;

namespace Flensrocker.ToDo.UnitTests.Tasks.TaskEvent;

[TestClass]
public sealed class StartedEventTests
{
  [TestMethod]
  public void IsJsonSerializable()
  {
    ToDoTestUtilities.AssertJsonSerializable<SuT, SuT.Started>(
      new SuT.Started { StartedAt = DateTimeOffset.UtcNow, ByUserId = Guid.CreateVersion7() }
    );
  }
}
