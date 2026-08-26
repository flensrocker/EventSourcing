using Flensrocker.EventSourcing.Abstractions;
using Flensrocker.ToDo.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Flensrocker.ToDo.Domain;

public static class ToDoDomainExtensions
{
  extension(IServiceCollection services)
  {
    public IServiceCollection AddToDo()
    {
      services.AddTransient<ToDoCommandHandler>();
      services.AddTransient<ICommandHandler<ToDoEvent, ToDoCommand, ToDoCommandState>>(sp => sp.GetRequiredService<ToDoCommandHandler>());

      return services;
    }
  }
}
