using Microsoft.AspNetCore.Mvc;
using OrleansPlayground.ToDo.Orleans.Abstractions;

namespace OrleansPlayground.ToDo.AspNetCore;

public static class ToDoEndpoints
{
  public static IEndpointRouteBuilder MapToDo(this IEndpointRouteBuilder builder)
  {
    builder.MapPost("/api/todo/memorize", static async ([FromBody] MemorizeToDoDto dto, IClusterClient client) =>
    {
      var toDoId = Guid.CreateVersion7();
      var grain = client.GetGrain<IToDoAggregateGrain>(toDoId);
      var result = await grain.Memorize(dto);
      return result
        ? Results.Created($"/api/todo/{toDoId}", new { ToDoId = toDoId })
        : Results.BadRequest();
    });

    builder.MapPut("/api/todo/{toDoId}/complete", static async (Guid toDoId, IClusterClient client) =>
    {
      var grain = client.GetGrain<IToDoAggregateGrain>(toDoId);
      var result = await grain.Complete();
      return result
        ? Results.Ok()
        : Results.BadRequest();
    });

    builder.MapPut("/api/todo/{toDoId}/dismiss", static async (Guid toDoId, [FromBody] DismissToDoDto dto, IClusterClient client) =>
    {
      var grain = client.GetGrain<IToDoAggregateGrain>(toDoId);
      var result = await grain.Dismiss(dto);
      return result
        ? Results.Ok()
        : Results.BadRequest();
    });

    builder.MapPut("/api/todo/{toDoId}/reopen", static async (Guid toDoId, IClusterClient client) =>
    {
      var grain = client.GetGrain<IToDoAggregateGrain>(toDoId);
      var result = await grain.Reopen();
      return result
        ? Results.Ok()
        : Results.BadRequest();
    });

    builder.MapGet("/api/todo/{toDoId}", static async (Guid toDoId, IClusterClient client) =>
    {
      var grain = client.GetGrain<IToDoDetailsViewGrain>(toDoId);
      var result = await grain.GetView();
      return result is not null
        ? Results.Ok(result)
        : Results.NotFound();
    });

    builder.MapGet("/api/todo", static async (IClusterClient client) =>
    {
      var tenantId = Guid.Empty;
      var grain = client.GetGrain<IToDoListViewGrain>(tenantId);
      var result = await grain.GetList();
      return Results.Ok(result);
    });

    return builder;
  }
}
