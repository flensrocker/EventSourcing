using SuT = Flensrocker.ToDo.Abstractions.Tasks.TaskCommand;

namespace Flensrocker.ToDo.UnitTests.Tasks.TaskCommand;

[TestClass]
public sealed class ChangeTitleCommandTests
{
  [TestMethod]
  [DataRow("Valid Title")]
  [DataRow("max length 23456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 1234567890")]
  public void GivenValidInputsThenReturnSuccess(string validTitle)
  {
    var testResult = SuT.ChangeTitle.Create(title: validTitle);

    var result = Assert.IsInstanceOfType<DomainResult<SuT.ChangeTitle>.Success>(testResult);
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
  public void GivenValidButDirtyInputsThenReturnSuccessWithSanitizedValues(string title, string expectedTitle)
  {
    var testResult = SuT.ChangeTitle.Create(title: title);

    var result = Assert.IsInstanceOfType<DomainResult<SuT.ChangeTitle>.Success>(testResult);
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
  public void GivenInvalidInputsThenReturnValidationProblem(string? invalidTitle)
  {
    var testResult = SuT.ChangeTitle.Create(title: invalidTitle);

    var result = Assert.IsInstanceOfType<DomainResult<SuT.ChangeTitle>.ValidationProblem>(testResult);
    Assert.IsTrue(result.Errors.ContainsKey(nameof(SuT.ChangeTitle.Title)));
  }

  [TestMethod]
  public void IsJsonSerializable()
  {
    ToDoTestUtilities.AssertJsonSerializable<SuT, SuT.ChangeTitle>(SuT.ChangeTitle.Create(title: "Some Title"));
  }
}
