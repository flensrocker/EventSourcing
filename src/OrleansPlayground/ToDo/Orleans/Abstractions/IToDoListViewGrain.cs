using Orleans;
using OrleansPlayground.ToDo.Views;

namespace OrleansPlayground.ToDo.Orleans.Abstractions;

public interface IToDoListViewGrain : IGrainWithGuidKey
{
  Task<ToDoListItemView[]> GetList();
}
