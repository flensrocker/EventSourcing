using Flensrocker.EventSourcing.Orleans.Abstractions;
using Flensrocker.ToDo.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Flensrocker.ToDo.Orleans.Client;

public static class ToDoOrleansClientExtensions
{
  extension(IServiceCollection services)
  {
    public IServiceCollection AddToDoOrleansClient()
    {
      services.AddCommandHandlerGrain<ToDoCommand>().AddProjectionGrain<ToDoView.Details>();

      return services;
    }
  }
}
