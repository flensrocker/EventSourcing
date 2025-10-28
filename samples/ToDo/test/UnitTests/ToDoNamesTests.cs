using Flensrocker.Domain.Abstractions;
using Flensrocker.ToDo.Abstractions;
using Flensrocker.ToDo.Orleans.Abstractions;

namespace Flensrocker.ToDo.UnitTests;

[TestClass]
public sealed class ToDoNamesTests
{
  [TestMethod]
  public void ToDoEventStreamNamespaceMatchesTypeName()
  {
    Assert.AreEqual(typeof(ToDoEvent).GetTypeName(), ToDoNames.ToDoEventStream);
  }
}
