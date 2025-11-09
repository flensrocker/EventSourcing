using Flensrocker.Domain.Abstractions;
using Flensrocker.ToDo.Abstractions;
using Flensrocker.ToDo.Abstractions.Views;
using Flensrocker.ToDo.Orleans.Abstractions;
using Flensrocker.ToDo.Persistence.PostgreSQL;
using Microsoft.EntityFrameworkCore;
using Orleans.Streams;
using Orleans.Streams.Core;

namespace Flensrocker.ToDo.Orleans.Server;

[ImplicitStreamSubscription(ToDoNames.ToDoEventStream)]
public class ToDoPersistListItemGrain(
  IDomainEventStorage<ToDoEvent> eventStore,
  IDbContextFactory<ToDoDbContext> dbContextFactory)
  : Grain, IGrainWithGuidKey, IStreamSubscriptionObserver, IAsyncObserver<DomainEvent<ToDoEvent>>
{
  private readonly IDomainEventStorage<ToDoEvent> _eventStore = eventStore;
  private readonly IDbContextFactory<ToDoDbContext> _dbContextFactory = dbContextFactory;

  private ToDoListItemView? _toDoItem;

  public async Task OnSubscribed(IStreamSubscriptionHandleFactory handleFactory)
  {
    var handle = handleFactory.Create<DomainEvent<ToDoEvent>>();
    await handle.ResumeAsync(this);
  }

  public async Task OnNextAsync(DomainEvent<ToDoEvent> domainEvent, StreamSequenceToken? token = null)
  {
    var toDoId = domainEvent.Metadata.StreamId;
    var expectedVersion = domainEvent.Metadata.StreamVersion - 1;

    using var dbContext = await _dbContextFactory.CreateDbContextAsync();

    if (_toDoItem is null)
      _toDoItem = await dbContext.ListItem.AsNoTracking().Where(e => e.ToDoId == toDoId).FirstOrDefaultAsync();

    var addToDbContext = true;
    if (_toDoItem is null)
      _toDoItem = new();
    else
      addToDbContext = false;

    var entity = dbContext.Attach(_toDoItem);

    if (_toDoItem.Version < expectedVersion)
    {
      var fromVersion = Math.Max(1, _toDoItem.Version);
      var itemEvents = await _eventStore.ReadEventsFromStorage(domainEvent.Metadata.StreamId, fromVersion, expectedVersion + 1, default);
      if (itemEvents is not null)
      {
        foreach (var itemEvent in itemEvents)
          _toDoItem.Apply(itemEvent);
      }
    }

    _toDoItem.Apply(domainEvent);

    if (addToDbContext)
      entity.State = EntityState.Added;

    await dbContext.SaveChangesAsync();
  }

  public Task OnErrorAsync(Exception ex)
  {
    // TODO add error handling
    return Task.CompletedTask;
  }
}
