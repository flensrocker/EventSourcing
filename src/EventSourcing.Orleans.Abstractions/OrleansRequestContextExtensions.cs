using Flensrocker.EventSourcing.Abstractions;

namespace Flensrocker.EventSourcing.Orleans.Abstractions;

public static class OrleansRequestContextExtensions
{
  public const string ActiveTenantKey = "ActiveTenant";
  public const string UserKey = "User";

  public static void SetActiveTenant(Guid? tenantId)
  {
    if (tenantId.HasValue)
      RequestContext.Set(ActiveTenantKey, tenantId.Value);
    else
      RequestContext.Remove(ActiveTenantKey);
  }

  public static Guid? GetActiveTenant()
  {
    return RequestContext.Get(ActiveTenantKey) is Guid tenantId ? tenantId : null;
  }

  public static void SetUser(User? user)
  {
    if (user is null)
      RequestContext.Remove(UserKey);
    else
      RequestContext.Set(UserKey, user);
  }

  public static User? GetUser()
  {
    return RequestContext.Get(UserKey) is User user ? user : null;
  }
}
