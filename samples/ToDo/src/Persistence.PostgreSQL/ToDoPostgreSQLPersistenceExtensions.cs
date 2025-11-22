using System.Reflection;
using Flensrocker.EventSourcing.Abstractions;
using Flensrocker.EventSourcing.Orleans.EntityFrameworkCore;
using Flensrocker.ToDo.Abstractions;
using Flensrocker.ToDo.Persistence.PostgreSQL.Queries;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
        npgsqlOptions.EnableRetryOnFailure(
          maxRetryCount: 10,
          maxRetryDelay: TimeSpan.FromSeconds(30),
          errorCodesToAdd: null);
      });
    });

    services.AddViewQuery<ListToDosViewQuery, ToDoQueryRequest.ListToDos, ToDoQueryResponse.ListToDos>();

    return services;
  }

  public static async Task MigrateToDoPostgreSQLPersistenceAsync(this IServiceProvider services)
  {
    var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(ToDoPostgreSQLPersistenceExtensions));
    var dbContextFactory = services.GetRequiredService<IDbContextFactory<ToDoDbContext>>();

    await using (var dbContext = await dbContextFactory.CreateDbContextAsync().ConfigureAwait(false))
      await dbContext.Database.MigrateAsync().ConfigureAwait(false);

    // TODO evaluate if the projection has to be rebuilt
    //var eventStore = services.GetRequiredService<IDomainEventStorage<ToDoEvent>>();
    //await dbContextFactory.RebuildProjectionAsync<ToDoDbContext, ToDoView.ToDoListItem, ToDoEvent>(eventStore, logger, default).ConfigureAwait(false);
  }
}
