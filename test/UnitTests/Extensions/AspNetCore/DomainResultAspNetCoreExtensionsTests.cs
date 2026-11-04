using System.Collections.Immutable;
using Flensrocker.EventSourcing.Extensions.AspNetCore;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Flensrocker.EventSourcing.UnitTests.Extensions.AspNetCore;

[TestClass]
public class DomainResultAspNetCoreExtensionsTests
{
  [TestMethod]
  public void GivenSuccessResultThenReturnHttpOkWithSameValue()
  {
    var result = new DomainResult<string>.Success { Value = "Success" };

    var typedResult = result.ToTypedResult();

    var httpResult = Assert.IsInstanceOfType<Ok<string>>(typedResult);
    Assert.AreEqual(result.Value, httpResult.Value);
  }

  [TestMethod]
  public async Task GivenSuccessResultTaskThenReturnHttpOkWithSameValue()
  {
    var result = new DomainResult<string>.Success { Value = "Success" };

    var typedResult = await Task.FromResult<DomainResult<string>>(result).ToTypedResult();

    var httpResult = Assert.IsInstanceOfType<Ok<string>>(typedResult);
    Assert.AreEqual(result.Value, httpResult.Value);
  }

  [TestMethod]
  public void GivenSuccessResultAndCreatedUriThenReturnHttpCreatedWithSameValue()
  {
    var result = new DomainResult<string>.Success { Value = "Success" };

    var location = "created";
    var typedResult = result.ToTypedResult(location);

    var httpResult = Assert.IsInstanceOfType<Created<string>>(typedResult);
    Assert.AreEqual(location, httpResult.Location);
    Assert.AreEqual(result.Value, httpResult.Value);
  }

  [TestMethod]
  public void GivenBadRequestResultThenReturnHttpBadRequest()
  {
    var result = new DomainResult<string>.BadRequest { Message = "Bad Request" };

    var typedResult = result.ToTypedResult();

    var httpResult = Assert.IsInstanceOfType<BadRequest<string>>(typedResult);
    Assert.AreEqual(result.Message, httpResult.Value);
  }

  [TestMethod]
  public void GivenForbidResultThenReturnHttpForbid()
  {
    var result = new DomainResult<string>.Forbid();

    var typedResult = result.ToTypedResult();

    Assert.IsInstanceOfType<ForbidHttpResult>(typedResult);
  }

  [TestMethod]
  public void GivenInternalErrorResultThenReturnHttpInternalServerError()
  {
    var result = new DomainResult<string>.InternalError { Message = "Internal Error" };

    var typedResult = result.ToTypedResult();

    var httpResult = Assert.IsInstanceOfType<InternalServerError<string>>(typedResult);
    Assert.AreEqual(result.Message, httpResult.Value);
  }

  [TestMethod]
  public void GivenNotFoundResultThenReturnHttpNotFound()
  {
    var result = new DomainResult<string>.NotFound { Message = "Not Found" };

    var typedResult = result.ToTypedResult();

    var httpResult = Assert.IsInstanceOfType<NotFound<string>>(typedResult);
    Assert.AreEqual(result.Message, httpResult.Value);
  }

  [TestMethod]
  public void GivenUnauthorizedResultThenReturnHttpUnauthorized()
  {
    var result = new DomainResult<string>.Unauthorized();

    var typedResult = result.ToTypedResult();

    Assert.IsInstanceOfType<UnauthorizedHttpResult>(typedResult);
  }

  [TestMethod]
  public void GivenValidationProblemResultThenReturnHttpValidationProblem()
  {
    var result = new DomainResult<string>.ValidationProblem
    {
      Title = "Title",
      Detail = "Detail",
      Errors = ImmutableDictionary<string, string[]>.Empty.Add("Key 1", ["Error 1a", "Error 1b"]).Add("Key 2", ["Error 2a"]),
    };

    var typedResult = result.ToTypedResult();

    var httpResult = Assert.IsInstanceOfType<ValidationProblem>(typedResult);
    Assert.AreEqual(result.Title, httpResult.ProblemDetails.Title);
    Assert.AreEqual(result.Detail, httpResult.ProblemDetails.Detail);
    Assert.AreSequenceEqual(result.Errors.ToArray(), httpResult.ProblemDetails.Errors.ToArray());
  }
}
