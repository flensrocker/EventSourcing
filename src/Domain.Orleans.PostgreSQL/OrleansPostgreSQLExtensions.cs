using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Flensrocker.Domain.Orleans.PostgreSQL;

public static class OrleansPostgreSQLExtensions
{
  public static IServiceCollection AddOrleansPostgreSQL(this IServiceCollection services, Func<OrleansPostgreSQLOptions> configureOptions)
  {
    var storageOptions = configureOptions();
    services.AddSingleton(storageOptions);

    services.AddDbContextFactory<OrleansDbContext>((serviceProvider, dbOptions) =>
    {
      var rootConfiguration = serviceProvider.GetRequiredService<IConfiguration>();
      var connectionString = rootConfiguration.GetConnectionString(storageOptions.ConnectionName);

      dbOptions.UseNpgsql(connectionString, npgsqlOptions =>
      {
        npgsqlOptions.MigrationsAssembly(Assembly.GetExecutingAssembly());
        npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", Constants.SchemaName);
      });
    });

    return services;
  }

  public static async Task MigrateOrleansPostgreSQLAsync(this IServiceProvider services)
  {
    var dbContextFactory = services.GetRequiredService<IDbContextFactory<OrleansDbContext>>();

    using var dbContext = dbContextFactory.CreateDbContext();
    await dbContext.Database.MigrateAsync().ConfigureAwait(false);
  }
}
