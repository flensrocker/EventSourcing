using System.Collections.Immutable;
using System.Net;
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

  [TestMethod]
  public async Task GivenAnAsyncSequenceOfDomainResultsWhenWrappingWithSseErrorThenReturnSequenceOfSseItems()
  {
    static async IAsyncEnumerable<DomainResult<string>> EnumerateResults()
    {
      yield return new DomainResult<string>.Success { Value = "Success" };
      yield return new DomainResult<string>.BadRequest { Message = "Bad Request" };
      yield return new DomainResult<string>.Forbid();
      yield return new DomainResult<string>.InternalError { Message = "Internal Error" };
      yield return new DomainResult<string>.NotFound { Message = "Not Found" };
      yield return new DomainResult<string>.Unauthorized();
      yield return new DomainResult<string>.ValidationProblem
      {
        Title = "Title",
        Detail = "Detail",
        Errors = ImmutableDictionary<string, string[]>.Empty.Add("Key 1", ["Error 1a", "Error 1b"]).Add("Key 2", ["Error 2a"]),
      };
    }

    var index = 0;

    await foreach (var sseItem in EnumerateResults().WithSseErrorEvents("EventType"))
    {
      switch (index)
      {
        case 0:
        {
          Assert.AreEqual("EventType", sseItem.EventType);
          var value = Assert.IsInstanceOfType<string>(sseItem.Data);
          Assert.AreEqual("Success", value);
          break;
        }
        case 1:
        {
          Assert.AreEqual(AspNetCoreConstants.SseErrorEventType, sseItem.EventType);
          var value = Assert.IsInstanceOfType<SseError>(sseItem.Data);
          Assert.AreEqual(HttpStatusCode.BadRequest, value.StatusCode);
          Assert.AreEqual("Bad Request", value.Message);
          break;
        }
        case 2:
        {
          Assert.AreEqual(AspNetCoreConstants.SseErrorEventType, sseItem.EventType);
          var value = Assert.IsInstanceOfType<SseError>(sseItem.Data);
          Assert.AreEqual(HttpStatusCode.Forbidden, value.StatusCode);
          break;
        }
        case 3:
        {
          Assert.AreEqual(AspNetCoreConstants.SseErrorEventType, sseItem.EventType);
          var value = Assert.IsInstanceOfType<SseError>(sseItem.Data);
          Assert.AreEqual(HttpStatusCode.InternalServerError, value.StatusCode);
          Assert.AreEqual("Internal Error", value.Message);
          break;
        }
        case 4:
        {
          Assert.AreEqual(AspNetCoreConstants.SseErrorEventType, sseItem.EventType);
          var value = Assert.IsInstanceOfType<SseError>(sseItem.Data);
          Assert.AreEqual(HttpStatusCode.NotFound, value.StatusCode);
          Assert.AreEqual("Not Found", value.Message);
          break;
        }
        case 5:
        {
          Assert.AreEqual(AspNetCoreConstants.SseErrorEventType, sseItem.EventType);
          var value = Assert.IsInstanceOfType<SseError>(sseItem.Data);
          Assert.AreEqual(HttpStatusCode.Unauthorized, value.StatusCode);
          break;
        }
        case 6:
        {
          Assert.AreEqual(AspNetCoreConstants.SseErrorEventType, sseItem.EventType);
          var value = Assert.IsInstanceOfType<SseError>(sseItem.Data);
          Assert.AreEqual(HttpStatusCode.BadRequest, value.StatusCode);
          Assert.AreEqual("Title", value.Message);
          break;
        }
      }

      index++;
    }
  }
}
