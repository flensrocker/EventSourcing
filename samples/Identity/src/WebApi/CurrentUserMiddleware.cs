using Flensrocker.EventSourcing.Abstractions;
using Flensrocker.EventSourcing.Orleans.Abstractions;
using Microsoft.AspNetCore.Http;

namespace Flensrocker.Identity.WebApi;

public class CurrentUserMiddleware(RequestDelegate next)
{
  private readonly RequestDelegate _next = next;

  public async Task InvokeAsync(HttpContext context)
  {
    var currentUser = UserExtensions.ToUser(context.User);
    OrleansRequestContextExtensions.SetUser(currentUser);

    await _next(context);
  }
}
