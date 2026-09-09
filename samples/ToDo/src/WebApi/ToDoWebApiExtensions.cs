using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Flensrocker.ToDo.WebApi;

public static class ToDoWebApiExtensions
{
  public static IEndpointRouteBuilder MapToDo(this IEndpointRouteBuilder builder)
  {
    // NOTE you can add group specific metadata like authentication etc. on the MapGroup
    var taskGroupBuilder = builder.MapGroup(ToDoRoutes.TaskRoute);
    taskGroupBuilder.MapTask(ToDoRoutes.TaskRoute);

    return builder;
  }
}
