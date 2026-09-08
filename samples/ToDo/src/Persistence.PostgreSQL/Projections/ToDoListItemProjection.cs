using Flensrocker.EventSourcing.Abstractions;
using Flensrocker.EventSourcing.Abstractions.PersistentProjection;
using Flensrocker.EventSourcing.Extensions.EntityFrameworkCore;
using Flensrocker.ToDo.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Flensrocker.ToDo.Persistence.PostgreSQL.Projections;

internal class ToDoListItemProjection(
  IDbContextFactory<ToDoDbContext> dbContextFactory,
  IDomainEventStorage<ToDoEvent> eventStore,
  ILogger<ToDoListItemProjection> logger,
  [FromKeyedServices(ToDoConstants.ToDoView.ListItem)] PersistentProjectionOptions projectionOptions
)
  : EntityFrameworkCorePersistentProjection<ToDoDbContext, ToDoView.ListItem, ToDoEvent>(
    dbContextFactory,
    eventStore,
    logger,
    projectionOptions
  ) { }
