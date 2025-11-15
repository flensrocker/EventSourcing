using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace Flensrocker.EventSourcing.Abstractions;

public static class DomainExtensions
{
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

  public static IServiceCollection AddViewQuery<TViewQuery, TQueryRequest, TQueryResponse>(this IServiceCollection services)
    where TViewQuery : class, IViewQuery<TQueryRequest, TQueryResponse>
  {
    services.AddScoped<TViewQuery>();
    services.AddScoped<IViewQuery<TQueryRequest, TQueryResponse>>(sp => sp.GetRequiredService<TViewQuery>());

    return services;
  }

  public static IServiceCollection AddDomainEventPublisher<TPublisher>(this IServiceCollection services)
    where TPublisher : class, IDomainEventPublisher
  {
    services.AddSingleton<TPublisher>();
    services.AddSingleton<IDomainEventPublisher>(sp => sp.GetRequiredService<TPublisher>());

    return services;
  }

  public static IServiceCollection AddDomainEventPublisher<TPublisher, TPublisherOptions>(this IServiceCollection services, Func<TPublisherOptions> configureOptions)
    where TPublisher : class, IDomainEventPublisher
    where TPublisherOptions : class
  {
    var options = configureOptions();
    services.AddSingleton(options);

    return services.AddDomainEventPublisher<TPublisher>();
  }
}
