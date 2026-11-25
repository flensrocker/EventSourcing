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
          static async Task<Results<BadRequest, NotFound, Ok<AuthSignedInDto>>> (HttpContext context, [FromBody] AuthSignInDto request) =>
          {
            if (string.IsNullOrWhiteSpace(request.DisplayName) || string.IsNullOrWhiteSpace(request.Email))
            {
              return TypedResults.BadRequest();
            }

            var user = AuthUserData.GetUserId(request.Email);
            if (!user.HasValue)
            {
              return TypedResults.NotFound();
            }

            var displayName = AuthUserData.NormalizeDisplayName(request.DisplayName);

            Claim[] claims =
            [
              new(ClaimTypes.NameIdentifier, user.Value.UserId.ToString()),
              new(ClaimTypes.Name, displayName),
              new(ClaimTypes.Email, user.Value.NormalizedEmail),
            ];
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            var authProperties = new AuthenticationProperties { IsPersistent = true };
            await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal, authProperties);

            return TypedResults.Ok(new AuthSignedInDto { DisplayName = displayName, Email = user.Value.NormalizedEmail });
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
}

internal static class AuthUserData
{
  private static readonly IReadOnlyDictionary<string, Guid> _emailUserIdMapping = new Dictionary<string, Guid>
  {
    { NormalizeEmail("user1@flensrocker.de"), Guid.Parse("11669e29-b187-4d68-b461-e4b99452fa44") },
    { NormalizeEmail("user1+1@flensrocker.de"), Guid.Parse("11669e29-b187-4d68-b461-e4b99452fa44") },
    { NormalizeEmail("user2@flensrocker.de"), Guid.Parse("ef59b503-f7b0-4433-b4d9-cae90d972fce") },
  };

  internal static string NormalizeDisplayName(string displayName)
  {
    return displayName.Trim();
  }

  internal static string NormalizeEmail(string email)
  {
    return MailboxAddress.EncodeAddrspec(email.Trim()).ToLowerInvariant();
  }

  internal static (string NormalizedEmail, Guid UserId)? GetUserId(string email)
  {
    var normalizedEmail = NormalizeEmail(email);
    return _emailUserIdMapping.TryGetValue(normalizedEmail, out var userId) ? (normalizedEmail, userId) : null;
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
