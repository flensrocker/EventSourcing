using Flensrocker.Domain.Orleans.Abstractions;
using Flensrocker.ToDo.Abstractions.Views;

namespace Flensrocker.ToDo.Orleans.Abstractions;

public interface IToDoDetailsViewGrain : IDomainEventProjectionGrain<ToDoDetailsView>
{
}
