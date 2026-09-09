using Flensrocker.ToDo.WebApi.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Flensrocker.ToDo.WebApi;

public static class ToDoWebApiExtensions
{
  extension(IServiceCollection services)
  {
    public IServiceCollection AddToDoWebApi()
    {
      services.AddValidation();

      return services;
    }
  }

  extension(IEndpointRouteBuilder builder)
  {
    public IEndpointRouteBuilder MapToDo()
    {
      builder.MapGroup(ToDoRoutes.TaskRoute).MapTasks(ToDoRoutes.TaskRoute);

      return builder;
    }
  }
}
