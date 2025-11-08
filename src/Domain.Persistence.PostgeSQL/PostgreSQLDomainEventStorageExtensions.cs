using Flensrocker.Domain.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Flensrocker.Domain.Persistence.PostgreSQL;

public static class PostgreSQLDomainEventStorageExtensions
{
  public static IServiceCollection AddPostgreSQLDomainEventStorage<TEventBase>(this IServiceCollection services, Func<PostgreSQLDomainEventStorageOptions<TEventBase>> configureOptions)
    where TEventBase : class
  {
    var options = configureOptions() ?? new PostgreSQLDomainEventStorageOptions<TEventBase>();
    services.AddSingleton(options);

    services.AddScoped<PostgreSQLDomainEventStorage<TEventBase>>();
    services.AddScoped<IDomainEventStorage<TEventBase>>(sp => sp.GetRequiredService<PostgreSQLDomainEventStorage<TEventBase>>());

    return services;
  }
}
