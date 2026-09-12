using Microsoft.AspNetCore.Http;

namespace Flensrocker.EventSourcing.Extensions.AspNetCore;

public static class DomainResultAspNetCoreExtensions
{
  extension<T>(DomainResult<T> result)
  {
    /// <param name="createdUri">Returns "Created" instead of "Ok" if not null and not empty</param>
    /// <returns></returns>
    public IResult ToTypedResult(string? createdUri = null)
    {
      return result switch
      {
        DomainResult<T>.BadRequest r => TypedResults.BadRequest(r.Message),
        DomainResult<T>.Forbid r => TypedResults.Forbid(),
        DomainResult<T>.InternalError r => TypedResults.InternalServerError(r.Message),
        DomainResult<T>.NotFound r => TypedResults.NotFound(r.Message),
        DomainResult<T>.Success r when !string.IsNullOrWhiteSpace(createdUri) => TypedResults.Created(createdUri, r.Value),
        DomainResult<T>.Success r => TypedResults.Ok(r.Value),
        DomainResult<T>.Unauthorized r => TypedResults.Unauthorized(),
        DomainResult<T>.ValidationProblem r => TypedResults.ValidationProblem(title: r.Title, detail: r.Detail, errors: r.Errors),
        _ => TypedResults.InternalServerError(),
      };
    }
  }

  extension<T>(Task<DomainResult<T>> asyncResult)
  {
    /// <param name="createdUri">Returns "Created" instead of "Ok" if not null and not empty</param>
    /// <returns></returns>
    public async Task<IResult> ToTypedResult(string? createdUri = null)
    {
      var result = await asyncResult.ConfigureAwait(false);
      return result.ToTypedResult(createdUri);
    }
  }
}
