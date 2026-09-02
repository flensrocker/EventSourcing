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
    Assert.AreEqual(ToDoConstants.ToDoEventStreamType, jsonOptions.GetTypeName(typeof(ToDoEvent)));
  }

  [TestMethod]
  public void ToDoEventStreamNamespaceCanBeConvertedToType()
  {
    var jsonOptions = EventSourcingJsonSerializerOptions.Default.AddToDoJsonOptions();
    var toDoEventType = jsonOptions.GetTypeFromName(ToDoConstants.ToDoEventStreamType);
    Assert.AreEqual(typeof(ToDoEvent), toDoEventType);
  }
}
