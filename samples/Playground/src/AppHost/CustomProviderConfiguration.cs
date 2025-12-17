using Aspire.Hosting.Orleans;

namespace Playground.AppHost;

internal sealed class CustomProviderConfiguration(string providerType, string? serviceKey = null, IResourceBuilder<IResourceWithConnectionString>? resource = null) : IProviderConfiguration
{
  internal static CustomProviderConfiguration Create(IResourceBuilder<IResourceWithConnectionString> resourceBuilder, string? customProviderType = null)
  {
    const string resource = "Resource";
    var serviceKey = resourceBuilder.Resource.Name;
    var resourceType = resourceBuilder.Resource.GetType().Name;

    // Use a simple transformation to get the provider type: remove the "Resource" suffix if it exists.
    var providerType = customProviderType ?? (resourceType.EndsWith(resource) ? resourceType[..^resource.Length] : resourceType);

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
