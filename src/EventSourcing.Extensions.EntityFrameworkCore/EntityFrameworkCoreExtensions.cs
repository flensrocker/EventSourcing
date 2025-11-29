using Flensrocker.EventSourcing.Abstractions;
using Flensrocker.EventSourcing.Extensions.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Flensrocker.EventSourcing.Extension.EntityFrameworkCore;

public static class EntityFrameworkCoreExtensions
{
  extension(IServiceCollection services)
  {
    public IServiceCollection AddEntityFrameworkCoreDbMigration<TDbContext>() where TDbContext : DbContext
      => services.AddDbMigration<EntityFrameworkCoreDbMigration<TDbContext>>();
  }
}
