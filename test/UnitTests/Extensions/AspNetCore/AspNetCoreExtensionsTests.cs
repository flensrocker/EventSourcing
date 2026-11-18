using Flensrocker.EventSourcing.Abstractions;
using Flensrocker.EventSourcing.Extensions.AspNetCore;

namespace Flensrocker.EventSourcing.UnitTests.Extensions.AspNetCore;

[TestClass]
public class AspNetCoreExtensionsTests
{
  [TestMethod]
  public void GivenAspNetCoreJsonSerializationOptionsThenSseErrorCanBeSerialized()
  {
    var jsonOptions = new EventSourcingJsonSerializerOptions().AddAspNetCoreJsonOptions();

    var hasTypeInfo = jsonOptions.Options.TryGetTypeInfo(typeof(SseError), out var sseErrorTypeInfo);
    Assert.IsTrue(hasTypeInfo);
    Assert.IsNotNull(sseErrorTypeInfo);
  }
}
