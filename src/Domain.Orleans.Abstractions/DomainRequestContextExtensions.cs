using Flensrocker.Domain.Abstractions;

namespace Flensrocker.Domain.Orleans.Abstractions;

public static class DomainRequestContextExtensions
{
  public const string UserKey = "User";

  public static void SetUser(DomainUser user)
  {
    RequestContext.Set(UserKey, user);
  }

  public static DomainUser? GetUser()
  {
    return RequestContext.Get(UserKey) is DomainUser user ? user : null;
  }
}
