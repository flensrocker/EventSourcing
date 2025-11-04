using System.Text.Json;
using System.Text.Json.Serialization;

namespace Flensrocker.Domain.Abstractions;

public static class DomainExtensions
{
  public static JsonSerializerOptions CreateBaseJsonSerializerOptions()
  {
    return new JsonSerializerOptions
    {
      AllowOutOfOrderMetadataProperties = true,
      PropertyNamingPolicy = null,
    };
  }

  public static JsonSerializerOptions WithTypeResolversOf(this JsonSerializerOptions options, JsonSerializerContext context)
  {
    options.TypeInfoResolverChain.Add(context);
    return options;
  }

  public static string GetTypeName(this Type? type)
  {
    return type is null || type.FullName is not string typeFullName || type.Assembly.GetName().Name is not string assemblyName
      ? ""
      : $"{typeFullName.Replace(".", "-")}_-_{assemblyName.Replace(".", "-")}";
  }

  public static Type? GetTypeFromName(string typeName)
  {
    if (string.IsNullOrWhiteSpace(typeName))
      return null;

    return Type.GetType(typeName.Replace("_-_", ", ").Replace("-", "."), false);
  }
}
