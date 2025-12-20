namespace Aspire.Hosting.Orleans;

public sealed class OrleansProviderTypeAnnotation : IResourceAnnotation
{
  /// <summary>
  /// The provider type of the resource that should be used instead of the default.
  /// </summary>
  public string ProviderType { get; internal set; } = "";
}

public static class OrleansServiceExtensions
{
  /// <summary>
  /// Adds a custom provider type to the resource that will be used for Orleans.
  /// </summary>
  /// <example>
  /// Garnet can be used as a drop-in replacement for Redis.
  /// Since Orleans only knows how to use the "Redis" client in its <c>RedisClusteringProviderBuilder</c> and other provider builders,
  /// we need to override the provider type of "Garnet" (derived from the type <c>GarnetResource</c>) to be "Redis".
  /// This does not only apply to clustering, but also to other Orleans providers that can use drop-in replacements,
  /// such as stream providers, grain directory providers and grain storage.
  /// <code>
  /// var garnet = builder.AddGarnet("garnet")
  ///   .WithOrleansProviderType("Redis");
  ///
  /// var orleans = builder.AddOrleans("orleans")
  ///   .WithClustering(garnet);
  /// </code>
  /// </example>
  /// <param name="builder">The builder of the resource that should be annotated.</param>
  /// <param name="providerType">The provider type to use.</param>
  /// <returns>The resource builder.</returns>
  public static IResourceBuilder<T> WithOrleansProviderType<T>(this IResourceBuilder<T> builder, string providerType)
      where T : IResource
  {
    return builder.WithAnnotation(new OrleansProviderTypeAnnotation { ProviderType = providerType });
  }
}

public sealed class CustomProviderConfiguration(string providerType, string? serviceKey = null, IResourceBuilder<IResourceWithConnectionString>? resource = null) : IProviderConfiguration
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
    var providerType = resourceType.EndsWith(resource) ? resourceType[..^resource.Length] : resourceType;

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
