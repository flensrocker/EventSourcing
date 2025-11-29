using System.Reflection;
using Flensrocker.EventSourcing.Extension.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Flensrocker.EventSourcing.Orleans.PostgreSQL;

public static class OrleansPostgreSQLExtensions
{
  extension(IServiceCollection services)
  {
    public IServiceCollection AddOrleansPostgreSQL(Func<OrleansPostgreSQLOptions> configureOptions)
    {
      var storageOptions = configureOptions();
      services.AddSingleton(storageOptions);

      services.AddPooledDbContextFactory<OrleansDbContext>((serviceProvider, dbOptions) =>
      {
        var rootConfiguration = serviceProvider.GetRequiredService<IConfiguration>();
        var connectionString = rootConfiguration.GetConnectionString(storageOptions.ConnectionName);

        dbOptions.UseNpgsql(connectionString, npgsqlOptions =>
        {
          npgsqlOptions.MigrationsAssembly(Assembly.GetExecutingAssembly());
          npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", Constants.SchemaName);
          npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 10,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorCodesToAdd: null);
        });
      });
      services.AddEntityFrameworkCoreDbMigration<OrleansDbContext>();

      return services;
    }
  }
}
