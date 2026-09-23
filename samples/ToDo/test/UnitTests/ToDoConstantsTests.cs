using Flensrocker.ToDo.Abstractions.Tasks;

namespace Flensrocker.ToDo.UnitTests;

[TestClass]
public sealed class ToDoConstantsTests
{
  [TestMethod]
  public void TaskEventStreamNamespaceMatchesTypeName()
  {
    Assert.AreEqual(TaskConstants.TaskEvent.Base, ToDoTestUtilities.JsonOptions.GetTypeName(typeof(TaskEvent)));
  }

  [TestMethod]
  public void TaskEventStreamNamespaceCanBeConvertedToType()
  {
    Assert.AreEqual(typeof(TaskEvent), ToDoTestUtilities.JsonOptions.GetTypeFromName(TaskConstants.TaskEvent.Base));
  }
}
