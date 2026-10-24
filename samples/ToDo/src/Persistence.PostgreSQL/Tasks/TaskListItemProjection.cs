using Flensrocker.EventSourcing.Abstractions;
using Flensrocker.EventSourcing.Abstractions.PersistentProjection;
using Flensrocker.EventSourcing.Extensions.EntityFrameworkCore;
using Flensrocker.ToDo.Abstractions.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Flensrocker.ToDo.Persistence.PostgreSQL.Tasks;

// NOTE BEST PRACTICES
// The IPersistentProjection (implemented in EntityFrameworkCorePersistentProjection) is for tracking the current version of the persisted projection.
// It's responsible for doing the rebuild while migrating the database during an application update.
// The IDbStrongConsistentProjection is for updating the projection itself while the application is running.
// Since the IDbStrongConsistentProjection is injected into the IDomainEventStorage,
// and the EntityFrameworkCorePersistentProjection needs the IDomainEventStorage for the rebuild,
// these two interfaces cannot be implemented in the same class.
// You don't even need IDbStrongConsistentProjection if you use a grain derived from EntityFrameworkCoreProjectionGrain for updating the projection asynchronously.

internal class TaskListItemPersistentProjection(
  IDbContextFactory<ToDoDbContext> dbContextFactory,
  IDomainEventStorage<TaskEvent> eventStore,
  ILogger<TaskListItemPersistentProjection> logger,
  [FromKeyedServices(TaskConstants.TaskView.ListItem)] PersistentProjectionOptions projectionOptions
)
  : EntityFrameworkCorePersistentProjection<ToDoDbContext, TaskView.ListItem, TaskEvent>(
    dbContextFactory,
    eventStore,
    logger,
    projectionOptions
  ) { }

internal class TaskListItemStrongConsistentProjection(ILogger<TaskListItemStrongConsistentProjection> logger)
  : IDbStrongConsistentProjection<TaskEvent, ToDoDbContext>
{
  private readonly ILogger<TaskListItemStrongConsistentProjection> _logger = logger;
  private TaskView.ListItem? _listItem = null;

  async Task IDbStrongConsistentProjection<TaskEvent, ToDoDbContext>.ApplyEvents(
    IDomainEventStorage<TaskEvent> eventStore,
    ToDoDbContext dbContext,
    IReadOnlyList<DomainEvent> domainEvents
  )
  {
    _listItem = await dbContext
      .UpdateProjectionAsync(eventStore, domainEvents, _listItem, TaskConstants.TaskView.ListItem, _logger)
      .ConfigureAwait(false);
  }
}
