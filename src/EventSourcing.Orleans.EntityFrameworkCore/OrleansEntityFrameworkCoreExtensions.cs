using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Flensrocker.EventSourcing.Orleans.EntityFrameworkCore;

public static class OrleansEntityFrameworkCoreExtensions
{
  extension(IServiceCollection services)
  {
    public IServiceCollection AddOrleansEntityFrameworkCoreProjections()
    {
      services.TryAddSingleton<OrleansEntityFrameworkCoreProjectionMetrics>();

      return services;
    }
  }
}
