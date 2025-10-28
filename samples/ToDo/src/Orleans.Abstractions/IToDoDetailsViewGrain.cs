using Flensrocker.ToDo.Abstractions.Views;

namespace Flensrocker.ToDo.Orleans.Abstractions;

public interface IToDoDetailsViewGrain : IGrainWithGuidKey
{
  Task<ToDoDetailsView?> GetView();
}
