using Flensrocker.EventSourcing.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Flensrocker.EventSourcing.Persistence.EntityFrameworkCore;

public class EntityFrameworkCoreDomainEventStorageBuilder
{
  private readonly IServiceCollection _services;

  public EntityFrameworkCoreDomainEventStorageBuilder(IServiceCollection services)
  {
    _services = services;
  }

  /// <summary>
  /// Use this overload if you want to implement strong consistent projections on your DbContext.
  /// The DbContext has to call <c>modelBuilder.ApplyDBPROVIDEREventSourcingConfiguration(excludeFromMigrations: true)</c> in its configuration.
  /// </summary>
  public EntityFrameworkCoreDomainEventStorageBuilder ForDomainEvents<TEventBase, TDbContext>()
    where TEventBase : class
    where TDbContext : DbContext
  {
    _services.AddScoped<EntityFrameworkCoreDomainEventStorage<TEventBase, TDbContext>>();
    _services.AddScoped<IDomainEventStorage<TEventBase>>(sp =>
      sp.GetRequiredService<EntityFrameworkCoreDomainEventStorage<TEventBase, TDbContext>>()
    );

    return this;
  }

  public EntityFrameworkCoreDomainEventStorageBuilder ForDomainEvents<TEventBase>()
    where TEventBase : class
  {
    return ForDomainEvents<TEventBase, EventSourcingDbContext>();
  }
}
