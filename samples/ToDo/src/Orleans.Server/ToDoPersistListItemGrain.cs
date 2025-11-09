using Flensrocker.Domain.Abstractions;
using Flensrocker.ToDo.Abstractions;
using Flensrocker.ToDo.Abstractions.Views;
using Flensrocker.ToDo.Orleans.Abstractions;
using Flensrocker.ToDo.Persistence.PostgreSQL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Orleans.Streams;
using Orleans.Streams.Core;

namespace Flensrocker.ToDo.Orleans.Server;

[ImplicitStreamSubscription(ToDoNames.ToDoEventStream)]
public class ToDoPersistListItemGrain(
  IDomainEventStorage<ToDoEvent> eventStore,
  IDbContextFactory<ToDoDbContext> dbContextFactory,
  ILogger<ToDoPersistListItemGrain> logger)
  : Grain, IGrainWithGuidKey, IStreamSubscriptionObserver, IAsyncObserver<DomainEvent<ToDoEvent>>
{
  private readonly IDomainEventStorage<ToDoEvent> _eventStore = eventStore;
  private readonly IDbContextFactory<ToDoDbContext> _dbContextFactory = dbContextFactory;
  private readonly ILogger<ToDoPersistListItemGrain> _logger = logger;

  private ToDoListItemView? _toDoItem;

  public async Task OnSubscribed(IStreamSubscriptionHandleFactory handleFactory)
  {
    var handle = handleFactory.Create<DomainEvent<ToDoEvent>>();
    await handle.ResumeAsync(this);
  }

  public async Task OnNextAsync(DomainEvent<ToDoEvent> domainEvent, StreamSequenceToken? token = null)
  {
    var entityTypeName = typeof(ToDoListItemView).GetTypeName();
    var entityId = domainEvent.Metadata.StreamId;
    var expectedVersion = domainEvent.Metadata.StreamVersion - 1;

    try
    {
      using var strategyDbContext = await _dbContextFactory.CreateDbContextAsync();

      await strategyDbContext.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
      {
        using var dbContext = await _dbContextFactory.CreateDbContextAsync();

        _toDoItem ??= await dbContext.ListItem.AsNoTracking().Where(e => e.ToDoId == entityId).FirstOrDefaultAsync();

        var addToDbContext = false;
        if (_toDoItem is null)
        {
          _toDoItem = new();
          addToDbContext = true;
        }
        else if (_toDoItem.Version > expectedVersion)
          return;

        var entity = dbContext.Attach(_toDoItem);

        if (_toDoItem.Version < expectedVersion)
        {
          _logger.LogWarning("Entity {EntityType}, entity-id {EntityId} is missing some events, expected version {ExpectedVersion}, got version {ActualVersion}", entityTypeName, entityId, expectedVersion, _toDoItem.Version);

          var fromVersion = Math.Max(1, _toDoItem.Version);
          var itemEvents = await _eventStore.ReadEventsFromStorage(entityId, fromVersion, expectedVersion + 1, default);
          if (itemEvents is null)
          {
            _logger.LogError("Failure in retrieving missing events from storage for type {EntityType}, entity-id {EntityId} and version range {FromVersion} to {ExpectedVersion}", entityTypeName, entityId, fromVersion, expectedVersion);
            return;
          }

          foreach (var itemEvent in itemEvents)
            _toDoItem.Apply(itemEvent);
        }

        _toDoItem.Apply(domainEvent);

        if (addToDbContext)
          entity.State = EntityState.Added;

        await dbContext.SaveChangesAsync();
      });
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failure in applying updates to projection of type {EntityType}, entity-id {EntityId} and expected version {ExpectedVersion}", entityTypeName, entityId, expectedVersion);
    }
  }

  public Task OnErrorAsync(Exception ex)
  {
    // TODO add error handling
    return Task.CompletedTask;
  }
}
