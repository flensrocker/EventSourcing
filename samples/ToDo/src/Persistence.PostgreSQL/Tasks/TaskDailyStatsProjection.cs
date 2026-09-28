using Flensrocker.EventSourcing.Abstractions;
using Flensrocker.EventSourcing.Abstractions.PersistentProjection;
using Flensrocker.EventSourcing.Extensions.EntityFrameworkCore;
using Flensrocker.ToDo.Abstractions.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Flensrocker.ToDo.Persistence.PostgreSQL.Tasks;

internal class TaskDailyStatsPersistentProjection(
  IDbContextFactory<ToDoDbContext> dbContextFactory,
  IDomainEventStorage<TaskEvent> eventStore,
  ILogger<TaskDailyStatsPersistentProjection> logger,
  [FromKeyedServices(TaskConstants.TaskView.DailyStats)] PersistentProjectionOptions projectionOptions
)
  : EntityFrameworkCorePersistentProjection<ToDoDbContext, TaskView.DailyStats, TaskEvent>(
    dbContextFactory,
    eventStore,
    logger,
    projectionOptions
  ) { }
