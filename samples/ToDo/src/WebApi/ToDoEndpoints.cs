using Flensrocker.Domain.Abstractions;
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
    var toDo = client.GetGrain<IToDoAggregateGrain>(toDoId);
    var result = await toDo.ExecuteCommand(command);
    return result;
  }

  public static IEndpointRouteBuilder MapToDo(this IEndpointRouteBuilder builder)
  {
    builder.MapPost("/api/todo/memorize", static async (
      [FromBody] MemorizeToDoDto dto,
      [FromServices] IClusterClient client) =>
    {
      var command = ToDoCommand.MemorizeToDo.Create(title: dto.Title);
      if (command is null)
        return Results.BadRequest();

      var toDoId = Guid.CreateVersion7();
      var result = await client.ExecuteCommandAsync(toDoId, command);
      return result
        ? Results.Created($"/api/todo/{toDoId}", new { ToDoId = toDoId })
        : Results.BadRequest();
    });

    builder.MapPut("/api/todo/{toDoId}/complete", static async (
      Guid toDoId,
      [FromServices] IClusterClient client) =>
    {
      var command = ToDoCommand.CompleteToDo.Create();
      if (command is null)
        return Results.BadRequest();

      var result = await client.ExecuteCommandAsync(toDoId, command);
      return result
        ? Results.Ok()
        : Results.BadRequest();
    });

    builder.MapPut("/api/todo/{toDoId}/dismiss", static async (
      Guid toDoId,
      [FromBody] DismissToDoDto dto,
      [FromServices] IClusterClient client) =>
    {
      var command = ToDoCommand.DismissToDo.Create(reason: dto.Reason);
      if (command is null)
        return Results.BadRequest();

      var result = await client.ExecuteCommandAsync(toDoId, command);
      return result
        ? Results.Ok()
        : Results.BadRequest();
    });

    builder.MapPut("/api/todo/{toDoId}/reopen", static async (
      Guid toDoId,
      [FromServices] IClusterClient client) =>
    {
      var command = ToDoCommand.ReopenToDo.Create();
      if (command is null)
        return Results.BadRequest();

      var result = await client.ExecuteCommandAsync(toDoId, command);
      return result
        ? Results.Ok()
        : Results.BadRequest();
    });

    builder.MapGet("/api/todo/{toDoId}", static async (
      Guid toDoId,
      [FromServices] IClusterClient client,
      CancellationToken cancellationToken) =>
    {
      var toDoDetails = client.GetGrain<IToDoDetailsViewGrain>(toDoId);
      var result = await toDoDetails.GetProjection(cancellationToken);
      return result is not null
        ? Results.Ok(result)
        : Results.NotFound();
    });

    builder.MapGet("/api/todo", static async (
      [FromServices] IViewQuery<ToDoQuery.ListToDosRequest, ToDoQuery.ListToDosResponse> query,
      CancellationToken cancellationToken) =>
    {
      var request = ToDoQuery.ListToDosRequest.Create(tenantId: null);
      if (request is null)
        return Results.BadRequest();

      var response = await query.ExecuteQueryAsync(request, cancellationToken);
      return Results.Ok(response);
    });

    return builder;
  }
}
