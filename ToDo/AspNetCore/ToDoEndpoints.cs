using Microsoft.AspNetCore.Mvc;
using OrleansPlayground.ToDo.Orleans;

namespace OrleansPlayground.ToDo.AspNetCore;

public static class ToDoEndpoints
{
  public static IEndpointRouteBuilder MapToDo(this IEndpointRouteBuilder builder)
  {
    builder.MapPost("/api/todo/memorize", static async ([FromBody] MemorizeToDoDto dto, IClusterClient client) =>
    {
      var toDoId = Guid.CreateVersion7();
      var toDoGrain = client.GetGrain<IToDoGrain>(toDoId);
      var toDoEvent = await toDoGrain.Memorize(dto);
      return toDoEvent ? Results.Created($"/api/todo/{toDoId}", new { ToDoId = toDoId }) : Results.BadRequest();
    });

    builder.MapPut("/api/todo/{toDoId}/complete", static async (Guid toDoId, IClusterClient client) =>
    {
      var toDoGrain = client.GetGrain<IToDoGrain>(toDoId);
      var toDoEvent = await toDoGrain.Complete();
      return toDoEvent ? Results.Ok() : Results.BadRequest();
    });

    builder.MapPut("/api/todo/{toDoId}/dismiss", static async (Guid toDoId, [FromBody] DismissToDoDto dto, IClusterClient client) =>
    {
      var toDoGrain = client.GetGrain<IToDoGrain>(toDoId);
      var toDoEvent = await toDoGrain.Dismiss(dto);
      return toDoEvent ? Results.Ok() : Results.BadRequest();
    });

    builder.MapPut("/api/todo/{toDoId}/reopen", static async (Guid toDoId, IClusterClient client) =>
    {
      var toDoGrain = client.GetGrain<IToDoGrain>(toDoId);
      var toDoEvent = await toDoGrain.Reopen();
      return toDoEvent ? Results.Ok() : Results.BadRequest();
    });

    builder.MapGet("/api/todo/{toDoId}", static async (Guid toDoId, IClusterClient client) =>
    {
      var toDoGrain = client.GetGrain<IToDoGrain>(toDoId);
      var toDo = await toDoGrain.GetToDo();
      return Results.Ok(toDo);
    });

    return builder;
  }
}
