using Aspire.Hosting.Orleans;

namespace Playground.AppHost;

public sealed class OrleansProviderTypeAnnotation : IResourceAnnotation
{
  /// <summary>
  /// The provider type of the resource that should be used instead of the default.
  /// </summary>
  public string ProviderType { get; internal set; } = "";
}

internal sealed class CustomProviderConfiguration(string providerType, string? serviceKey = null, IResourceBuilder<IResourceWithConnectionString>? resource = null) : IProviderConfiguration
{
  internal static CustomProviderConfiguration Create(IResourceBuilder<IResourceWithConnectionString> resourceBuilder)
  {
    var serviceKey = resourceBuilder.Resource.Name;

    if (resourceBuilder.Resource.TryGetAnnotationsOfType<OrleansProviderTypeAnnotation>(out var annotations) && annotations.FirstOrDefault() is OrleansProviderTypeAnnotation annotation)
    {
      return new(annotation.ProviderType, serviceKey, resourceBuilder);
    }

    const string resource = "Resource";
    var resourceType = resourceBuilder.Resource.GetType().Name;

    // Use a simple transformation to get the provider type: remove the "Resource" suffix if it exists.
    var providerType = (resourceType.EndsWith(resource) ? resourceType[..^resource.Length] : resourceType);

    return new(providerType, serviceKey, resourceBuilder);
  }

  public void ConfigureResource<T>(IResourceBuilder<T> resourceBuilder, string configurationSectionName) where T : IResourceWithEnvironment
  {
    var envVarPrefix = configurationSectionName.Replace(":", "__");
    resourceBuilder.WithEnvironment($"Orleans__{envVarPrefix}__ProviderType", providerType);
    if (!string.IsNullOrEmpty(serviceKey))
    {
      resourceBuilder.WithEnvironment($"Orleans__{envVarPrefix}__ServiceKey", serviceKey);
    }

    if (resource is not null)
    {
      resourceBuilder.WithReference(resource);
    }
  }
}

internal static class CustomProviderConfigurationExtensions
{
  public static IResourceBuilder<T> WithOrleansProviderType<T>(this IResourceBuilder<T> builder, string providerType)
      where T : IResource
  {
    return builder.WithAnnotation(new OrleansProviderTypeAnnotation { ProviderType = providerType });
  }
}
