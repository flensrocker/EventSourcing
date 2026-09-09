using Flensrocker.EventSourcing.Abstractions;
using Flensrocker.EventSourcing.Abstractions.PersistentProjection;
using Flensrocker.EventSourcing.Extensions.EntityFrameworkCore;
using Flensrocker.ToDo.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Flensrocker.ToDo.Persistence.PostgreSQL.Projections;

internal class TaskListItemProjection(
  IDbContextFactory<ToDoDbContext> dbContextFactory,
  IDomainEventStorage<TaskEvent> eventStore,
  ILogger<TaskListItemProjection> logger,
  [FromKeyedServices(ToDoConstants.TaskView.ListItem)] PersistentProjectionOptions projectionOptions
)
  : EntityFrameworkCorePersistentProjection<ToDoDbContext, TaskView.ListItem, TaskEvent>(
    dbContextFactory,
    eventStore,
    logger,
    projectionOptions
  ) { }
