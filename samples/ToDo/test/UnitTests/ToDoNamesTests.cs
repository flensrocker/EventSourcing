using Flensrocker.EventSourcing.Abstractions;
using Flensrocker.ToDo.Abstractions;
using Flensrocker.ToDo.Orleans.Abstractions;

namespace Flensrocker.ToDo.UnitTests;

[TestClass]
public sealed class ToDoNamesTests
{
  [TestMethod]
  public void ToDoEventStreamNamespaceMatchesTypeName()
  {
    Assert.AreEqual(ToDoNames.ToDoEventStream, typeof(ToDoEvent).GetTypeName());
  }

  [TestMethod]
  public void ToDoEventStreamNamespaceCanBeConvertedToType()
  {
    var toDoEventType = DomainExtensions.GetTypeFromName(ToDoNames.ToDoEventStream);
    Assert.AreEqual(typeof(ToDoEvent), toDoEventType);
  }
}
