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
      services.AddSingleton<ILifecycleParticipant<IClusterClientLifecycle>>(sp => sp.GetRequiredService<OrleansDomainEventStreamPublisher>());
      services.AddSingleton<ILifecycleParticipant<ISiloLifecycle>>(sp => sp.GetRequiredService<OrleansDomainEventStreamPublisher>());

      return services;
    }
  }
}
