// NOTE BEST PRACTICE
// The namespace of the tests and of the "system under test" (SuT) are the same after the assembly prefix.
// To avoid name conflicts use a "using alias" for the SuT.
using SuT = Flensrocker.ToDo.Abstractions.Tasks.TaskCommand;

namespace Flensrocker.ToDo.UnitTests.Tasks.TaskCommand;

[TestClass]
public sealed class DismissCommandTests
{
  [TestMethod]
  [DataRow("Valid Reason")]
  [DataRow(
    "max length 23456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 1234567890"
  )]
  public void GivenValidInputsThenReturnSuccess(string validReason)
  {
    var testResult = SuT.Dismiss.Create(reason: validReason);

    var result = Assert.IsInstanceOfType<DomainResult<SuT.Dismiss>.Success>(testResult);
    Assert.AreEqual(validReason, result.Value.Reason);
  }

  [TestMethod]
  [DataRow("Whitespace at end ", "Whitespace at end")]
  [DataRow(" Whitespace at front", "Whitespace at front")]
  [DataRow(" Whitespace at front and end ", "Whitespace at front and end")]
  [DataRow(
    " max length 23456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 1234567890",
    "max length 23456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 1234567890"
  )]
  public void GivenValidButDirtyInputsThenReturnSuccessWithSanitizedValues(string reason, string expectedReason)
  {
    var testResult = SuT.Dismiss.Create(reason: reason);

    var result = Assert.IsInstanceOfType<DomainResult<SuT.Dismiss>.Success>(testResult);
    Assert.AreEqual(expectedReason, result.Value.Reason);
  }

  [TestMethod]
  [DataRow((string?)null)]
  [DataRow("")]
  [DataRow(" ")]
  [DataRow("\t")]
  [DataRow(" \t")]
  [DataRow("\t ")]
  [DataRow(" \t ")]
  [DataRow(
    "123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 1"
  )]
  [DataRow("With\nLinebreak")]
  [DataRow("With\rCarriage Return")]
  public void GivenInvalidInputsThenReturnValidationProblem(string? invalidReason)
  {
    var testResult = SuT.Dismiss.Create(reason: invalidReason);

    var result = Assert.IsInstanceOfType<DomainResult<SuT.Dismiss>.ValidationProblem>(testResult);
    Assert.IsTrue(result.Errors.ContainsKey(nameof(SuT.Dismiss.Reason)));
  }

  [TestMethod]
  public void IsJsonSerializable()
  {
    ToDoTestUtilities.AssertJsonSerializable<SuT, SuT.Dismiss>(SuT.Dismiss.Create(reason: "Some Reason"));
  }
}
