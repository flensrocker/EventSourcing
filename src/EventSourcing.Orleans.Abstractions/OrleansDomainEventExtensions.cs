using Flensrocker.EventSourcing.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Flensrocker.EventSourcing.Orleans.Abstractions;

public static class OrleansDomainEventExtensions
{
  extension(IServiceCollection services)
  {
    public IServiceCollection AddOrleansDomainEventStreamPublisher()
    {
      services.AddSingleton<OrleansDomainEventStreamPublisherMetrics>();
      services.AddDomainEventPublisher<OrleansDomainEventStreamPublisher>();
      services.AddSingleton<ILifecycleParticipant<IClusterClientLifecycle>>(sp =>
        sp.GetRequiredService<OrleansDomainEventStreamPublisher>()
      );
      services.AddSingleton<ILifecycleParticipant<ISiloLifecycle>>(sp => sp.GetRequiredService<OrleansDomainEventStreamPublisher>());

      return services;
    }

    public IServiceCollection AddCommandHandlerGrain<TCommandBase>()
    {
      services.AddKeyedScoped<ICommandHandler<TCommandBase>>(
        KeyedService.AnyKey,
        (sp, key) =>
        {
          if (key is not Guid id)
            throw new ArgumentException("Key must be Guid");

          var client = sp.GetRequiredService<IClusterClient>();
          return client.GetGrain<ICommandHandlerGrain<TCommandBase>>(id);
        }
      );

      return services;
    }

    public IServiceCollection AddProjectionGrain<TProjection>()
    {
      services.AddKeyedScoped<IDomainEventProjection<TProjection>>(
        KeyedService.AnyKey,
        (sp, key) =>
        {
          if (key is not Guid id)
            throw new ArgumentException("Key must be Guid");

          var client = sp.GetRequiredService<IClusterClient>();
          return client.GetGrain<IDomainEventProjectionGrain<TProjection>>(id);
        }
      );

      return services;
    }
  }
}
