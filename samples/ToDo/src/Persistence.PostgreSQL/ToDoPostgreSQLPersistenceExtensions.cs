using System.Reflection;
using Flensrocker.EventSourcing.Abstractions;
using Flensrocker.EventSourcing.Extensions.EntityFrameworkCore;
using Flensrocker.EventSourcing.Persistence.EntityFrameworkCore;
using Flensrocker.ToDo.Abstractions.Tasks;
using Flensrocker.ToDo.Persistence.PostgreSQL.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Flensrocker.ToDo.Persistence.PostgreSQL;

public static class ToDoPostgreSQLPersistenceExtensions
{
  extension(IServiceCollection services)
  {
    public IServiceCollection AddToDoPostgreSQLPersistence(Func<ToDoPostgreSQLPersistenceOptions> configureOptions)
    {
      var storageOptions = configureOptions();
      services.AddSingleton(storageOptions);

      services.AddPooledDbContextFactory<ToDoDbContext>(
        (serviceProvider, dbOptions) =>
        {
          var rootConfiguration = serviceProvider.GetRequiredService<IConfiguration>();
          var connectionString = rootConfiguration.GetConnectionString(storageOptions.ConnectionName);

          dbOptions.UseNpgsql(connectionString, npgsqlOptions =>
          {
            npgsqlOptions.MigrationsAssembly(Assembly.GetExecutingAssembly());
            npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, ToDoPostgreSQLPersistenceConstants.SchemaName);
            npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 10, maxRetryDelay: TimeSpan.FromSeconds(30), errorCodesToAdd: null);
          });
        }
      );

      services.AddEntityFrameworkCoreDbMigration<ToDoDbContext>();
      services.AddEntityFrameworkCorePersistentProjection<
        TaskDailyStatsPersistentProjection,
        ToDoDbContext,
        TaskView.DailyStats,
        TaskEvent
      >(TaskConstants.TaskView.DailyStats, TaskView.DailyStats.ProjectionVersion);
      services.AddEntityFrameworkCorePersistentProjection<TaskListItemPersistentProjection, ToDoDbContext, TaskView.ListItem, TaskEvent>(
        TaskConstants.TaskView.ListItem,
        TaskView.ListItem.ProjectionVersion
      );
      services.AddEntityFrameworkCoreStrongConsistentProjection<TaskEvent, ToDoDbContext, TaskListItemStrongConsistentProjection>();

      services.AddViewQuery<DailyStatsTasksViewQuery, TaskQueryRequest.DailyStats, TaskQueryResponse.DailyStats>();
      services.AddViewQuery<ListTasksViewQuery, TaskQueryRequest.List, TaskQueryResponse.List>();

      return services;
    }
  }

  extension(EntityFrameworkCoreDomainEventStorageBuilder builder)
  {
    public EntityFrameworkCoreDomainEventStorageBuilder ForToDoEvents()
    {
      return builder.ForDomainEvents<TaskEvent, ToDoDbContext>();
    }
  }
}
