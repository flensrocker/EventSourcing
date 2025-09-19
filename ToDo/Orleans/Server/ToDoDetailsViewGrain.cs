using OrleansPlayground.Abstractions.EntityFrameworkCore;
using OrleansPlayground.ToDo.Domain;
using OrleansPlayground.ToDo.Orleans.Abstractions;
using OrleansPlayground.ToDo.Views;

namespace OrleansPlayground.ToDo.Orleans.Server;

public class ToDoDetailsViewGrain : Grain, IToDoDetailsViewGrain
{
  private readonly IDomainEventStore<Guid, ToDoEvent> _eventStore;

  private int _viewVersion;
  private ToDoDetailsView? _view;

  public ToDoDetailsViewGrain(IDomainEventStore<Guid, ToDoEvent> eventStore)
  {
    _eventStore = eventStore;
  }

  public override async Task OnActivateAsync(CancellationToken cancellationToken)
  {
    await base.OnActivateAsync(cancellationToken);

    var (version, view) = await _eventStore.ReadStateFromStorage<ToDoDetailsView>(this.GetPrimaryKey());

    _viewVersion = version;
    if (version > 0)
      _view = view;

    // TODO Subscribe ToDo-Events => update View
  }

  public Task<ToDoDetailsView?> GetView()
  {
    return Task.FromResult(_view);
  }
}
