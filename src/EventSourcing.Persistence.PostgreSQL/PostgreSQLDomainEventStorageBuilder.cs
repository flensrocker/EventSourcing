using Flensrocker.EventSourcing.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Flensrocker.EventSourcing.Persistence.PostgreSQL;

public class PostgreSQLDomainEventStorageBuilder
{
  private readonly IServiceCollection _services;

  internal PostgreSQLDomainEventStorageBuilder(IServiceCollection services)
  {
    _services = services;
  }

  public PostgreSQLDomainEventStorageBuilder ForDomainEvents<TEventBase>()
    where TEventBase : class
  {
    _services.AddScoped<PostgreSQLDomainEventStorage<TEventBase>>();
    _services.AddScoped<IDomainEventStorage<TEventBase>>(sp => sp.GetRequiredService<PostgreSQLDomainEventStorage<TEventBase>>());

    return this;
  }
}
