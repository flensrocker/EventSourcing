using Flensrocker.EventSourcing.Orleans.Abstractions;
using Flensrocker.ToDo.Abstractions;

namespace Flensrocker.ToDo.Orleans.Abstractions;

public interface IToDoCommandHandlerGrain : ICommandHandlerGrain<ToDoCommand>
{
}
