using Flensrocker.EventSourcing.Abstractions;
using Flensrocker.ToDo.Abstractions;

namespace Flensrocker.ToDo.UnitTests;

[TestClass]
public sealed class ToDoConstantsTests
{
  [TestMethod]
  public void ToDoEventStreamNamespaceMatchesTypeName()
  {
    Assert.AreEqual(ToDoConstants.ToDoEventStream, typeof(ToDoEvent).GetTypeName());
  }

  [TestMethod]
  public void ToDoEventStreamNamespaceCanBeConvertedToType()
  {
    var toDoEventType = EventSourcingExtensions.GetTypeFromName(ToDoConstants.ToDoEventStream);
    Assert.AreEqual(typeof(ToDoEvent), toDoEventType);
  }
}
