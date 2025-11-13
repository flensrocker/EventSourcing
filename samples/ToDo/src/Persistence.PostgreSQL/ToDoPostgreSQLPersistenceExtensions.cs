using System.Reflection;
using Flensrocker.Domain.Abstractions;
using Flensrocker.ToDo.Abstractions;
using Flensrocker.ToDo.Persistence.PostgreSQL.Queries;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Flensrocker.ToDo.Persistence.PostgreSQL;

public static class ToDoPostgreSQLPersistenceExtensions
{
  public static IServiceCollection AddToDoPostgreSQLPersistence(this IServiceCollection services, Func<ToDoPostgreSQLPersistenceOptions> configureOptions)
  {
    var storageOptions = configureOptions();
    services.AddSingleton(storageOptions);

    services.AddPooledDbContextFactory<ToDoDbContext>((serviceProvider, dbOptions) =>
    {
      var rootConfiguration = serviceProvider.GetRequiredService<IConfiguration>();
      var connectionString = rootConfiguration.GetConnectionString(storageOptions.ConnectionName);

      dbOptions.UseNpgsql(connectionString, npgsqlOptions =>
      {
        npgsqlOptions.MigrationsAssembly(Assembly.GetExecutingAssembly());
        npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", Constants.SchemaName);
        npgsqlOptions.EnableRetryOnFailure();
      });
    });

    services.AddViewQuery<ListToDosViewQuery, ToDoQueryRequest.ListToDos, ToDoQueryResponse.ListToDos>();

    return services;
  }

  public static async Task MigrateToDoPostgreSQLPersistenceAsync(this IServiceProvider services)
  {
    var dbContextFactory = services.GetRequiredService<IDbContextFactory<ToDoDbContext>>();

    using var dbContext = dbContextFactory.CreateDbContext();
    await dbContext.Database.MigrateAsync().ConfigureAwait(false);
  }
}
