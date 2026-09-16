// NOTE BEST PRACTICE
// The namespace of the tests and of the "system under test" (SuT) are the same after the assembly prefix.
// To avoid name conflicts use a "using alias" for the SuT.
using SuT = Flensrocker.ToDo.Abstractions.Tasks.TaskCommand;

namespace Flensrocker.ToDo.UnitTests.Tasks.TaskCommand;

[TestClass]
public sealed class NoteTests
{
  [TestMethod]
  [DataRow("Valid Title")]
  [DataRow("max length 23456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 1234567890")]
  public void ValidInputsReturnSuccess(string validTitle)
  {
    var testResult = SuT.Note.Create(title: validTitle);

    var result = Assert.IsInstanceOfType<DomainResult<SuT.Note>.Success>(testResult);
    Assert.AreEqual(validTitle, result.Value.Title);
  }

  [TestMethod]
  [DataRow("Whitespace at end ", "Whitespace at end")]
  [DataRow(" Whitespace at front", "Whitespace at front")]
  [DataRow(" Whitespace at front and end ", "Whitespace at front and end")]
  [DataRow(
    " max length 23456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 1234567890",
    "max length 23456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 1234567890"
  )]
  public void TitleIsSanitized(string title, string expectedTitle)
  {
    var testResult = SuT.Note.Create(title: title);

    var result = Assert.IsInstanceOfType<DomainResult<SuT.Note>.Success>(testResult);
    Assert.AreEqual(expectedTitle, result.Value.Title);
  }

  [TestMethod]
  [DataRow((string?)null)]
  [DataRow("")]
  [DataRow(" ")]
  [DataRow("\t")]
  [DataRow(" \t")]
  [DataRow("\t ")]
  [DataRow(" \t ")]
  [DataRow("123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 1")]
  [DataRow("With\nLinebreak")]
  [DataRow("With\rCarriage Return")]
  public void InvalidInputsReturnValidationProblem(string? invalidTitle)
  {
    var testResult = SuT.Note.Create(title: invalidTitle);

    var result = Assert.IsInstanceOfType<DomainResult<SuT.Note>.ValidationProblem>(testResult);
    Assert.IsTrue(result.Errors.ContainsKey(nameof(SuT.Note.Title)));
  }
}
