using Flensrocker.EventSourcing.Abstractions;
using Flensrocker.EventSourcing.Extensions.EntityFrameworkCore;
using Flensrocker.ToDo.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Flensrocker.ToDo.Persistence.PostgreSQL.Projections;

internal class ToDoListItemProjection(
  IDbContextFactory<ToDoDbContext> dbContextFactory,
  IDomainEventStorage<ToDoEvent> eventStore,
  ILogger<ToDoListItemProjection> logger)
  : EntityFrameworkCorePersistentProjection<ToDoDbContext, ToDoView.ToDoListItem, ToDoEvent>(dbContextFactory, eventStore, logger)
{
}
