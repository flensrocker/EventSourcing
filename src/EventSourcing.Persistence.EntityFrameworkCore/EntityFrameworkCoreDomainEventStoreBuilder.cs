using Flensrocker.EventSourcing.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Flensrocker.EventSourcing.Persistence.EntityFrameworkCore;

public class EntityFrameworkCoreDomainEventStoreBuilder
{
  private readonly IServiceCollection _services;

  public EntityFrameworkCoreDomainEventStoreBuilder(IServiceCollection services)
  {
    _services = services;
  }

  /// <summary>
  /// Use this overload if you want to implement strong consistent projections on your DbContext.
  /// The DbContext has to call <c>modelBuilder.ApplyDBPROVIDEREventSourcingConfiguration(excludeFromMigrations: true)</c> in its configuration.
  /// </summary>
  public EntityFrameworkCoreDomainEventStoreBuilder ForDomainEvents<TEventBase, TDbContext>()
    where TEventBase : class
    where TDbContext : DbContext
  {
    _services.AddScoped<EntityFrameworkCoreDomainEventStore<TEventBase, TDbContext>>();
    _services.AddScoped<IDomainEventStore<TEventBase>>(sp =>
      sp.GetRequiredService<EntityFrameworkCoreDomainEventStore<TEventBase, TDbContext>>()
    );

    return this;
  }

  public EntityFrameworkCoreDomainEventStoreBuilder ForDomainEvents<TEventBase>()
    where TEventBase : class
  {
    return ForDomainEvents<TEventBase, EventSourcingDbContext>();
  }
}
