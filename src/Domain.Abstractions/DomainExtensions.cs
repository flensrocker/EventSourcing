namespace Flensrocker.Domain.Abstractions;

public static class DomainExtensions
{
  public static string GetTypeName(this Type? type)
  {
    return type is null
      ? ""
      : $"{type.FullName}, {type.Assembly.GetName().Name}";
  }
}
