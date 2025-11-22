using Flensrocker.EventSourcing.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Flensrocker.EventSourcing.Orleans.Abstractions;

public static class OrleansDomainEventExtensions
{
  public static IServiceCollection AddOrleansDomainEventStreamPublisher(this IServiceCollection services)
  {
    services.AddSingleton<OrleansDomainEventStreamPublisherMetrics>();
    services.AddDomainEventPublisher<OrleansDomainEventStreamPublisher>();

    return services;
  }
}
