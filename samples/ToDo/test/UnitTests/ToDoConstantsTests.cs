using Flensrocker.EventSourcing.Abstractions;
using Flensrocker.ToDo.Abstractions;

namespace Flensrocker.ToDo.UnitTests;

[TestClass]
public sealed class ToDoConstantsTests
{
  [TestMethod]
  public void ToDoEventStreamNamespaceMatchesTypeName()
  {
    var jsonOptions = EventSourcingJsonSerializerOptions.Default.AddToDoJsonOptions();
    Assert.AreEqual(ToDoConstants.ToDoEvent.Base, jsonOptions.GetTypeName(typeof(ToDoEvent)));
  }

  [TestMethod]
  public void ToDoEventStreamNamespaceCanBeConvertedToType()
  {
    var jsonOptions = EventSourcingJsonSerializerOptions.Default.AddToDoJsonOptions();
    var toDoEventType = jsonOptions.GetTypeFromName(ToDoConstants.ToDoEvent.Base);
    Assert.AreEqual(typeof(ToDoEvent), toDoEventType);
  }
}
