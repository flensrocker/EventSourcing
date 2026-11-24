using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using MimeKit;

namespace Flensrocker.Identity.WebApi;

// just a dummy implementation to mimic authenticated users
public static class IdentityWebApiExtensions
{
  extension(IServiceCollection services)
  {
    public IServiceCollection AddIdentityWebApi()
    {
      services
        .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(options =>
        {
          options.AccessDeniedPath = "/identity/auth/forbidden";
          options.LoginPath = "/identity/auth/signin";
          options.LogoutPath = "/identity/auth/signout";
        });
      services.AddAuthorization();

      return services;
    }
  }

  extension(IApplicationBuilder app)
  {
    public IApplicationBuilder UseIdentityWebApi()
    {
      app.UseCookiePolicy(new CookiePolicyOptions { HttpOnly = HttpOnlyPolicy.Always, MinimumSameSitePolicy = SameSiteMode.Strict });
      app.UseAuthentication();
      app.UseAuthorization();

      return app;
    }
  }

  extension(IEndpointRouteBuilder builder)
  {
    public IEndpointRouteBuilder MapIdentityWebApi()
    {
      var authGroup = builder.MapGroup("/api/identity/auth");

      authGroup
        .MapPost(
          "signin",
          static async Task<Results<BadRequest, Ok<AuthSignedInDto>>> (HttpContext context, [FromBody] AuthSignInDto request) =>
          {
            if (string.IsNullOrWhiteSpace(request.DisplayName) || string.IsNullOrWhiteSpace(request.Email))
            {
              return TypedResults.BadRequest();
            }

            var displayName = NormalizeDisplayName(request.DisplayName);
            var email = NormalizeEmail(request.Email);

            Claim[] claims = [new(ClaimTypes.Name, displayName), new(ClaimTypes.Email, email)];
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            var authProperties = new AuthenticationProperties { IsPersistent = true };
            await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal, authProperties);

            return TypedResults.Ok(new AuthSignedInDto { DisplayName = displayName, Email = email });
          }
        )
        .AllowAnonymous();

      authGroup
        .MapPut("signout", static async (HttpContext context) =>
        {
          await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

          return TypedResults.Ok();
        })
        .AllowAnonymous();

      return builder;
    }
  }

  private static string NormalizeDisplayName(string displayName)
  {
    return displayName.Trim();
  }

  private static string NormalizeEmail(string email)
  {
    return MailboxAddress.EncodeAddrspec(email.Trim()).ToLowerInvariant();
  }
}

public record AuthSignInDto
{
  public string? DisplayName { get; init; }
  public string? Email { get; init; }
}

public record AuthSignedInDto
{
  public required string DisplayName { get; init; }
  public required string Email { get; init; }
}
