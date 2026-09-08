using Flensrocker.EventSourcing.Abstractions;
using Flensrocker.EventSourcing.Orleans.Abstractions;
using Flensrocker.ToDo.Abstractions;
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

  public static IEndpointRouteBuilder MapToDo(this IEndpointRouteBuilder builder)
  {
    builder.MapPost(
      "/api/todo/memorize",
      static async (HttpContext context, [FromBody] MemorizeToDoDto dto) =>
      {
        var command = ToDoCommand.MemorizeToDo.Create(title: dto.Title);
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
        var command = ToDoCommand.CompleteToDo.Create();
        if (command is null)
          return Results.BadRequest();

        var result = await context.RequestServices.ExecuteCommandAsync<ToDoCommand>(toDoId, command);
        return result ? Results.Ok() : Results.BadRequest();
      }
    );

    builder.MapPut(
      "/api/todo/{toDoId}/dismiss",
      static async (HttpContext context, Guid toDoId, [FromBody] DismissToDoDto dto) =>
      {
        var command = ToDoCommand.DismissToDo.Create(reason: dto.Reason);
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
        var command = ToDoCommand.ReopenToDo.Create();
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
      static async (Guid toDoId, [FromServices] IClusterClient client, CancellationToken cancellationToken) =>
      {
        var toDoDetails = client.GetGrain<IDomainEventProjectionGrain<ToDoView.ToDoDetails>>(toDoId);
        var result = await toDoDetails.GetProjection(cancellationToken);
        return result is null ? Results.NotFound() : Results.Ok(result);
      }
    );

    builder.MapGet(
      "/api/todo/{toDoId}/stream",
      static (Guid toDoId, [FromServices] IClusterClient client, CancellationToken cancellationToken) =>
      {
        var toDoDetails = client.GetGrain<IDomainEventProjectionGrain<ToDoView.ToDoDetails>>(toDoId);
        var toDoStream = toDoDetails.StreamProjection(cancellationToken);

        return Results.ServerSentEvents(toDoStream, eventType: nameof(ToDoView.ToDoDetails));
      }
    );

    builder.MapGet(
      "/api/todo",
      static async (
        [FromServices] IViewQuery<ToDoQueryRequest.ListToDos, ToDoQueryResponse.ListToDos> query,
        CancellationToken cancellationToken
      ) =>
      {
        var request = ToDoQueryRequest.ListToDos.Create(tenantId: null);
        if (request is null)
          return Results.BadRequest();

        var response = await query.ExecuteQueryAsync(request, cancellationToken);
        return Results.Ok(response);
      }
    );

    return builder;
  }
}
