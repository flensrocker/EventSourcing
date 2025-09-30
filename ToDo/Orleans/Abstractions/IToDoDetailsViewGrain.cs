using OrleansPlayground.ToDo.Views;

namespace OrleansPlayground.ToDo.Orleans.Abstractions;

public interface IToDoDetailsViewGrain : IGrainWithGuidKey
{
  Task<ToDoDetailsView?> GetView();
}
