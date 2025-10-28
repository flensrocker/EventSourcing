using Flensrocker.Domain.Orleans.Abstractions;
using Flensrocker.ToDo.Abstractions;
using Flensrocker.ToDo.Orleans.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Flensrocker.ToDo.WebApi;

public static class ToDoEndpoints
{
  public record MemorizeToDoDto
  {
    public string? Title { get; init; }
  }

  public record DismissToDoDto
  {
    public string? Reason { get; init; }
  }

  private static async Task<bool> ExecuteCommandAsync(this IClusterClient client, Guid toDoId, ToDoCommand command)
  {
    var toDo = client.GetGrain<IDomainAggegrateGrain<ToDoCommand>>(toDoId);
    var result = await toDo.ExecuteCommand(command);
    return result;
  }

  public static IEndpointRouteBuilder MapToDo(this IEndpointRouteBuilder builder)
  {
    builder.MapPost("/api/todo/memorize", static async ([FromBody] MemorizeToDoDto dto, IClusterClient client) =>
    {
      var command = ToDoCommand.MemorizeToDo.Create(dto.Title);
      if (command is null)
        return Results.BadRequest();

      var toDoId = Guid.CreateVersion7();
      var result = await client.ExecuteCommandAsync(toDoId, command);
      return result
        ? Results.Created($"/api/todo/{toDoId}", new { ToDoId = toDoId })
        : Results.BadRequest();
    });

    builder.MapPut("/api/todo/{toDoId}/complete", static async (Guid toDoId, IClusterClient client) =>
    {
      var command = ToDoCommand.CompleteToDo.Create();
      if (command is null)
        return Results.BadRequest();

      var result = await client.ExecuteCommandAsync(toDoId, command);
      return result
        ? Results.Ok()
        : Results.BadRequest();
    });

    builder.MapPut("/api/todo/{toDoId}/dismiss", static async (Guid toDoId, [FromBody] DismissToDoDto dto, IClusterClient client) =>
    {
      var command = ToDoCommand.DismissToDo.Create(dto.Reason);
      if (command is null)
        return Results.BadRequest();

      var result = await client.ExecuteCommandAsync(toDoId, command);
      return result
        ? Results.Ok()
        : Results.BadRequest();
    });

    builder.MapPut("/api/todo/{toDoId}/reopen", static async (Guid toDoId, IClusterClient client) =>
    {
      var command = ToDoCommand.ReopenToDo.Create();
      if (command is null)
        return Results.BadRequest();

      var result = await client.ExecuteCommandAsync(toDoId, command);
      return result
        ? Results.Ok()
        : Results.BadRequest();
    });

    builder.MapGet("/api/todo/{toDoId}", static async (Guid toDoId, IClusterClient client) =>
    {
      var toDoDetails = client.GetGrain<IToDoDetailsViewGrain>(toDoId);
      var result = await toDoDetails.GetView();
      return result is not null
        ? Results.Ok(result)
        : Results.NotFound();
    });

    builder.MapGet("/api/todo", static async (IClusterClient client) =>
    {
      var tenantId = Guid.Empty;
      var toDoList = client.GetGrain<IToDoListViewGrain>(tenantId);
      var result = await toDoList.GetList();
      return Results.Ok(result);
    });

    return builder;
  }
}
