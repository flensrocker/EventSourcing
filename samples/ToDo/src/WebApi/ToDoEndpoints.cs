using Flensrocker.EventSourcing.Abstractions;
using Flensrocker.ToDo.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Flensrocker.ToDo.WebApi;

public static class ToDoEndpoints
{
  public record MemorizeDto
  {
    public string? Title { get; init; }
  }

  public record DismissDto
  {
    public string? Reason { get; init; }
  }

  public static IEndpointRouteBuilder MapToDo(this IEndpointRouteBuilder builder)
  {
    builder.MapPost(
      "/api/todo/memorize",
      static async (HttpContext context, [FromBody] MemorizeDto dto) =>
      {
        var command = ToDoCommand.Memorize.Create(title: dto.Title);
        if (command is null)
          return Results.BadRequest();

        var toDoId = Guid.CreateVersion7();
        var result = await context.RequestServices.ExecuteCommandAsync<ToDoCommand>(toDoId, command);
        return result ? Results.Created($"/api/todo/{toDoId}", new { ToDoId = toDoId }) : Results.BadRequest();
      }
    );

    builder.MapPut(
      "/api/todo/{toDoId}/complete",
      static async (HttpContext context, Guid toDoId) =>
      {
        var command = ToDoCommand.Complete.Create();
        if (command is null)
          return Results.BadRequest();

        var result = await context.RequestServices.ExecuteCommandAsync<ToDoCommand>(toDoId, command);
        return result ? Results.Ok() : Results.BadRequest();
      }
    );

    builder.MapPut(
      "/api/todo/{toDoId}/dismiss",
      static async (HttpContext context, Guid toDoId, [FromBody] DismissDto dto) =>
      {
        var command = ToDoCommand.Dismiss.Create(reason: dto.Reason);
        if (command is null)
          return Results.BadRequest();

        var result = await context.RequestServices.ExecuteCommandAsync<ToDoCommand>(toDoId, command);
        return result ? Results.Ok() : Results.BadRequest();
      }
    );

    builder.MapPut(
      "/api/todo/{toDoId}/reopen",
      static async (HttpContext context, Guid toDoId) =>
      {
        var command = ToDoCommand.Reopen.Create();
        if (command is null)
          return Results.BadRequest();

        var result = await context.RequestServices.ExecuteCommandAsync<ToDoCommand>(toDoId, command);
        return result ? Results.Ok() : Results.BadRequest();
      }
    );

    builder.MapGet(
      "/api/todo/events",
      static ([FromServices] IDomainEventStorage<ToDoEvent> eventStore, CancellationToken cancellationToken) =>
      {
        var toDoEvents = eventStore.ReadAllEventsFromStorage(cancellationToken);
        return Results.ServerSentEvents(toDoEvents, eventType: nameof(ToDoEvent));
      }
    );

    builder.MapGet(
      "/api/todo/{toDoId}/events",
      static async (
        Guid toDoId,
        [FromQuery] string? mode,
        [FromQuery] int? from,
        [FromQuery] int? to,
        [FromServices] IDomainEventStorage<ToDoEvent> eventStore,
        CancellationToken cancellationToken
      ) =>
      {
        from ??= 1;
        to ??= 0;

        if (from.Value <= 0 || to.Value < 0 || (to.Value > 0 && to.Value <= from.Value))
          return Results.BadRequest();

        var toDoEvents = await eventStore.ReadEventsFromStorage(toDoId, from.Value, to.Value, cancellationToken);
        if (toDoEvents is null)
          return Results.NotFound();

        return Results.Ok(
          mode switch
          {
            "payload" => toDoEvents.Select(ev => ev.Payload).ToArray(),
            _ => toDoEvents,
          }
        );
      }
    );

    builder.MapGet(
      "/api/todo/{toDoId}",
      static async (HttpContext context, Guid toDoId, CancellationToken cancellationToken) =>
      {
        var result = await context.RequestServices.GetProjection<ToDoView.Details>(toDoId, cancellationToken);
        return result is null ? Results.NotFound() : Results.Ok(result);
      }
    );

    builder.MapGet(
      "/api/todo/{toDoId}/stream",
      static (HttpContext context, Guid toDoId, CancellationToken cancellationToken) =>
      {
        var toDoStream = context.RequestServices.StreamProjection<ToDoView.Details>(toDoId, cancellationToken);

        return Results.ServerSentEvents(toDoStream, eventType: nameof(ToDoView.Details));
      }
    );

    builder.MapGet(
      "/api/todo",
      static async ([FromServices] IViewQuery<ToDoQueryRequest.List, ToDoQueryResponse.List> query, CancellationToken cancellationToken) =>
      {
        var request = ToDoQueryRequest.List.Create(tenantId: null);
        if (request is null)
          return Results.BadRequest();

        var response = await query.ExecuteQueryAsync(request, cancellationToken);
        return Results.Ok(response);
      }
    );

    return builder;
  }
}
