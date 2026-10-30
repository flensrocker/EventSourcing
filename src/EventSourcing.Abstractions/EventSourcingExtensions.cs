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

  extension(IServiceCollection services)
  {
    public IServiceCollection AddFlensrockerEventSourcingCore()
    {
      services.TryAddSingleton(_ => new EventSourcingActivitySource());
      services.TryAddSingleton<DomainEventPublisherMetrics>();

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

    public async Task<DomainResult<CommandResult>> ExecuteCommandAsync<TCommandBase>(Guid id, TCommandBase command)
    {
      var handler = serviceProvider.GetRequiredKeyedService<ICommandHandler<TCommandBase>>(id);
      return await handler.ExecuteCommand(command).ConfigureAwait(false);
    }

    public async Task<DomainResult<TProjection>> GetProjection<TProjection>(Guid id, CancellationToken cancellationToken)
    {
      var handler = serviceProvider.GetRequiredKeyedService<IDomainEventProjection<TProjection>>(id);
      return await handler.GetProjection(cancellationToken).ConfigureAwait(false);
    }

    public IAsyncEnumerable<DomainResult<TProjection>> StreamProjection<TProjection>(Guid id, CancellationToken cancellationToken)
    {
      var handler = serviceProvider.GetRequiredKeyedService<IDomainEventProjection<TProjection>>(id);
      return handler.StreamProjection(cancellationToken);
    }
  }
}
