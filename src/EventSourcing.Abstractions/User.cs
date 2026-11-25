using System.Security.Claims;

namespace Flensrocker.EventSourcing.Abstractions;

[GenerateSerializer]
[Alias("Flensrocker_EventSourcing_UserClaim")]
public sealed record UserClaim
{
  [Id(0)]
  public required string Type { get; init; }

  [Id(1)]
  public required string Value { get; init; }
}

[GenerateSerializer]
[Alias("Flensrocker_EventSourcing_User")]
public sealed record User
{
  [Id(0)]
  public required Guid UserId { get; init; }

  [Id(1)]
  public IReadOnlyCollection<UserClaim> Claims { get; init; } = [];
}

public static class UserExtensions
{
  extension(ClaimsPrincipal? claimsPrincipal)
  {
    public User? ToUser()
    {
      if (
        claimsPrincipal?.Identity is null
        || !claimsPrincipal.Identity.IsAuthenticated
        || claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier) is not Claim userIdClaim
        || !Guid.TryParse(userIdClaim.Value, out var userId)
      )
      {
        return null;
      }

      return new User
      {
        UserId = userId,
        Claims = claimsPrincipal.Claims.Select(c => new UserClaim { Type = c.Type, Value = c.Value }).ToArray(),
      };
    }
  }
}
