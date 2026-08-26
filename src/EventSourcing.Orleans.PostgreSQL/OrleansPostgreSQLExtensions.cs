using System.Reflection;
using Flensrocker.EventSourcing.Extensions.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
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
          npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Constants.SchemaName);
        });
      });
      services.AddEntityFrameworkCoreDbMigration<OrleansDbContext>();

      return services;
    }
  }
}
