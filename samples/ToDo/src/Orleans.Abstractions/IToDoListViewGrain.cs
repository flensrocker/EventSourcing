using Flensrocker.ToDo.Abstractions.Views;

namespace Flensrocker.ToDo.Orleans.Abstractions;

public interface IToDoListViewGrain : IGrainWithGuidKey
{
  Task<ToDoListItemView[]> GetList();
}
