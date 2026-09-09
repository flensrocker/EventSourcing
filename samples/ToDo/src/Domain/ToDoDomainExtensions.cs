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
      services.AddTransient<TaskCommandHandler>();
      services.AddTransient<ICommandHandler<TaskEvent, TaskCommand, TaskCommandState>>(sp => sp.GetRequiredService<TaskCommandHandler>());

      return services;
    }
  }
}
