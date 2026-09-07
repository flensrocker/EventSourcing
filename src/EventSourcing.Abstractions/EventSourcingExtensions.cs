using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Flensrocker.EventSourcing.Abstractions;

public static class EventSourcingExtensions
{
  extension(JsonSerializerOptions options)
  {
    public JsonSerializerOptions WithTypeResolversOf(JsonSerializerContext context)
    {
      options.TypeInfoResolverChain.Add(context);
      return options;
    }
  }

  [Obsolete("Will be migrated to EventSourcingJsonSerializerOptions.GetTypeName")]
  public static string GetTypeName(this Type? type)
  {
    return type is null || type.FullName is not string typeFullName || type.Assembly.GetName().Name is not string assemblyName
      ? ""
      : $"{typeFullName.Replace(".", "-")}_-_{assemblyName.Replace(".", "-")}";
  }

  [Obsolete("Will be migrated to EventSourcingJsonSerializerOptions.GetTypeFromName")]
  public static Type? GetTypeFromName(string typeName)
  {
    if (string.IsNullOrWhiteSpace(typeName))
      return null;

    return Type.GetType(typeName.Replace("_-_", ", ").Replace("-", "."), false);
  }

  extension(IServiceCollection services)
  {
    public IServiceCollection AddFlensrockerEventSourcingCore()
    {
      services.TryAddSingleton(_ => new EventSourcingActivitySource());

      return services;
    }

    public IServiceCollection AddViewQuery<TViewQuery, TQueryRequest, TQueryResponse>()
      where TViewQuery : class, IViewQuery<TQueryRequest, TQueryResponse>
    {
      services.AddScoped<TViewQuery>();
      services.AddScoped<IViewQuery<TQueryRequest, TQueryResponse>>(sp => sp.GetRequiredService<TViewQuery>());

      return services;
    }

    public IServiceCollection AddDomainEventPublisher<TPublisher>()
      where TPublisher : class, IDomainEventPublisher
    {
      services.AddSingleton<TPublisher>();
      services.AddSingleton<IDomainEventPublisher>(sp => sp.GetRequiredService<TPublisher>());

      return services;
    }

    public IServiceCollection AddDomainEventPublisher<TPublisher, TPublisherOptions>(Func<TPublisherOptions> configureOptions)
      where TPublisher : class, IDomainEventPublisher
      where TPublisherOptions : class
    {
      var options = configureOptions();
      services.AddSingleton(options);

      return services.AddDomainEventPublisher<TPublisher>();
    }

    public IServiceCollection AddNoopDomainEventPublisher() => services.AddDomainEventPublisher<NoopDomainEventPublisher>();

    public IServiceCollection AddDbMigration<TDbMigration>()
      where TDbMigration : class, IDbMigration
    {
      services.AddScoped<TDbMigration>();
      services.AddScoped<IDbMigration>(sp => sp.GetRequiredService<TDbMigration>());

      return services;
    }
  }

  extension(IServiceProvider serviceProvider)
  {
    public async Task RunDbMigrationsAsync()
    {
      var dbMigrations = serviceProvider.GetServices<IDbMigration>();

      foreach (var dbMigration in dbMigrations)
        await dbMigration.MigrateAsync().ConfigureAwait(false);
    }
  }
}
