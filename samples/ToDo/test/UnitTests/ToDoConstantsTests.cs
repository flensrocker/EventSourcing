using Flensrocker.EventSourcing.Abstractions;
using Flensrocker.ToDo.Abstractions;
using Flensrocker.ToDo.Abstractions.Tasks;

namespace Flensrocker.ToDo.UnitTests;

[TestClass]
public sealed class ToDoConstantsTests
{
  [TestMethod]
  public void TaskEventStreamNamespaceMatchesTypeName()
  {
    var jsonOptions = EventSourcingJsonSerializerOptions.Default.AddToDoJsonOptions();
    Assert.AreEqual(TaskConstants.TaskEvent.Base, jsonOptions.GetTypeName(typeof(TaskEvent)));
  }

  [TestMethod]
  public void TaskEventStreamNamespaceCanBeConvertedToType()
  {
    var jsonOptions = EventSourcingJsonSerializerOptions.Default.AddToDoJsonOptions();
    Assert.AreEqual(typeof(TaskEvent), jsonOptions.GetTypeFromName(TaskConstants.TaskEvent.Base));
  }
}
