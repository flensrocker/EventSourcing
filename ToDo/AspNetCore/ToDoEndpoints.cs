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
      var grain = client.GetGrain<IToDoGrain>(toDoId);
      var @event = await grain.Memorize(dto);
      return @event
        ? Results.Created($"/api/todo/{toDoId}", new { ToDoId = toDoId })
        : Results.BadRequest();
    });

    builder.MapPut("/api/todo/{toDoId}/complete", static async (Guid toDoId, IClusterClient client) =>
    {
      var grain = client.GetGrain<IToDoGrain>(toDoId);
      var @event = await grain.Complete();
      return @event
        ? Results.Ok()
        : Results.BadRequest();
    });

    builder.MapPut("/api/todo/{toDoId}/dismiss", static async (Guid toDoId, [FromBody] DismissToDoDto dto, IClusterClient client) =>
    {
      var grain = client.GetGrain<IToDoGrain>(toDoId);
      var @event = await grain.Dismiss(dto);
      return @event
        ? Results.Ok()
        : Results.BadRequest();
    });

    builder.MapPut("/api/todo/{toDoId}/reopen", static async (Guid toDoId, IClusterClient client) =>
    {
      var grain = client.GetGrain<IToDoGrain>(toDoId);
      var @event = await grain.Reopen();
      return @event
        ? Results.Ok()
        : Results.BadRequest();
    });

    builder.MapGet("/api/todo/{toDoId}", static async (Guid toDoId, IClusterClient client) =>
    {
      var grain = client.GetGrain<IToDoDetailsViewGrain>(toDoId);
      var view = await grain.GetView();
      return view is not null
        ? Results.Ok(view)
        : Results.NotFound();
    });

    return builder;
  }
}
