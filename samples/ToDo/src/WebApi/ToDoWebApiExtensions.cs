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
      // NOTE you can add group specific metadata like authentication etc. on the MapGroup
      var taskGroupBuilder = builder.MapGroup(ToDoRoutes.TaskRoute);
      taskGroupBuilder.MapTask(ToDoRoutes.TaskRoute);

      return builder;
    }
  }
}
