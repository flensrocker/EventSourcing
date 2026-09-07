using System.Reflection;
using Flensrocker.EventSourcing.Abstractions;
using Flensrocker.EventSourcing.Extensions.EntityFrameworkCore;
using Flensrocker.ToDo.Abstractions;
using Flensrocker.ToDo.Persistence.PostgreSQL.Projections;
using Flensrocker.ToDo.Persistence.PostgreSQL.Queries;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Flensrocker.ToDo.Persistence.PostgreSQL;

public static class ToDoPostgreSQLPersistenceExtensions
{
  extension(IServiceCollection services)
  {
    public IServiceCollection AddToDoPostgreSQLPersistence(
      Func<ToDoPostgreSQLPersistenceOptions> configureOptions
    )
    {
      var storageOptions = configureOptions();
      services.AddSingleton(storageOptions);

      services.AddPooledDbContextFactory<ToDoDbContext>(
        (serviceProvider, dbOptions) =>
        {
          var rootConfiguration = serviceProvider.GetRequiredService<IConfiguration>();
          var connectionString = rootConfiguration.GetConnectionString(
            storageOptions.ConnectionName
          );

          dbOptions.UseNpgsql(
            connectionString,
            npgsqlOptions =>
            {
              npgsqlOptions.MigrationsAssembly(Assembly.GetExecutingAssembly());
              npgsqlOptions.MigrationsHistoryTable(
                HistoryRepository.DefaultTableName,
                Constants.SchemaName
              );
              npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 10,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorCodesToAdd: null
              );
            }
          );
        }
      );

      services.AddEntityFrameworkCoreDbMigration<ToDoDbContext>();
      services.AddEntityFrameworkCorePersistentProjection<
        ToDoListItemProjection,
        ToDoDbContext,
        ToDoView.ToDoListItem,
        ToDoEvent
      >(ToDoConstants.ToDoView.ToDoListItem, ToDoView.ToDoListItem.ProjectionVersion);

      services.AddViewQuery<
        ListToDosViewQuery,
        ToDoQueryRequest.ListToDos,
        ToDoQueryResponse.ListToDos
      >();

      return services;
    }
  }
}
