using Flensrocker.EventSourcing.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Flensrocker.EventSourcing.Persistence.PostgreSQL;

public class PostgreSQLDomainEventStorageBuilder
{
  private readonly IServiceCollection _services;

  internal PostgreSQLDomainEventStorageBuilder(IServiceCollection services)
  {
    _services = services;
  }

  /// <summary>
  /// Use this overload if you want to implement strong consistent projections on your DbContext.
  /// The DbContext has to call <c>modelBuilder.ApplyEventSourcingConfiguration(excludeFromMigrations: true)</c> in its configuration.
  /// </summary>
  public PostgreSQLDomainEventStorageBuilder ForDomainEvents<TEventBase, TDbContext>()
    where TEventBase : class
    where TDbContext : DbContext
  {
    _services.AddScoped<PostgreSQLDomainEventStorage<TEventBase, TDbContext>>();
    _services.AddScoped<IDomainEventStorage<TEventBase>>(sp =>
      sp.GetRequiredService<PostgreSQLDomainEventStorage<TEventBase, TDbContext>>()
    );

    return this;
  }

  public PostgreSQLDomainEventStorageBuilder ForDomainEvents<TEventBase>()
    where TEventBase : class
  {
    return ForDomainEvents<TEventBase, EventSourcingDbContext>();
  }
}
