using Flensrocker.Domain.Orleans.Abstractions;
using Flensrocker.ToDo.Abstractions;

namespace Flensrocker.ToDo.Orleans.Abstractions;

public interface IToDoAggregateGrain : IDomainAggegrateGrain<ToDoCommand>
{
}
