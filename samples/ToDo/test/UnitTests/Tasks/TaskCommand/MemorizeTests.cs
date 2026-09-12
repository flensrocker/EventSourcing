using SuT = Flensrocker.ToDo.Abstractions.Tasks.TaskCommand;

namespace Flensrocker.ToDo.UnitTests.Tasks.TaskCommand;

[TestClass]
public sealed class MemorizeTests
{
  [TestMethod]
  [DataRow("Valid Title")]
  [DataRow("123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 1234567890")]
  public void ValidTitleReturnsSuccess(string validTitle)
  {
    var testResult = SuT.Memorize.Create(title: validTitle);

    var result = Assert.IsInstanceOfType<DomainResult<SuT.Memorize>.Success>(testResult);
    Assert.AreEqual(validTitle, result.Value.Title);
  }

  [TestMethod]
  [DataRow((string?)null)]
  [DataRow("")]
  [DataRow("123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 1")]
  [DataRow("With\nLinebreak")]
  [DataRow("With\rCarriage Return")]
  public void InvalidTitleReturnsValidationProblem(string? invalidTitle)
  {
    var testResult = SuT.Memorize.Create(title: invalidTitle);

    var result = Assert.IsInstanceOfType<DomainResult<SuT.Memorize>.ValidationProblem>(testResult);
    Assert.IsTrue(result.Errors.ContainsKey(nameof(SuT.Memorize.Title)));
  }
}
