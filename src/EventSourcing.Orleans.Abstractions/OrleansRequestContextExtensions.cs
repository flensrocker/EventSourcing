using Flensrocker.EventSourcing.Abstractions;

namespace Flensrocker.EventSourcing.Orleans.Abstractions;

public static class OrleansRequestContextExtensions
{
  public const string UserKey = "User";

  public static void SetUser(User user)
  {
    RequestContext.Set(UserKey, user);
  }

  public static User? GetUser()
  {
    return RequestContext.Get(UserKey) is User user ? user : null;
  }
}
