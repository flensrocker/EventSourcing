using System.ComponentModel.DataAnnotations;
using Flensrocker.EventSourcing.Abstractions;
using Flensrocker.ToDo.Abstractions.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Flensrocker.ToDo.WebApi.Tasks;

public static class TaskEndpoints
{
  public record DismissDto
  {
    [Required]
    [MaxLength(TaskConstants.TaskDismissReasonMaxLength)]
    public string? Reason { get; init; }
  }

  public record MemorizeDto
  {
    [Required]
    [MaxLength(TaskConstants.TaskTitleMaxLength)]
    public string? Title { get; init; }
  }

  internal static TBuilder MapTasks<TBuilder>(this TBuilder builder, string taskRoute)
    where TBuilder : IEndpointRouteBuilder
  {
    // endpoints for commands
    builder.MapPut(
      "{taskId}/complete",
      static async (HttpContext context, Guid taskId) =>
      {
        var command = TaskCommand.Complete.Create();
        if (command is null)
          return Results.BadRequest();

        var result = await context.RequestServices.ExecuteCommandAsync<TaskCommand>(taskId, command);
        return result ? Results.Ok() : Results.BadRequest();
      }
    );

    builder.MapPut(
      "{taskId}/dismiss",
      static async (HttpContext context, Guid taskId, [FromBody] DismissDto dto) =>
      {
        var command = TaskCommand.Dismiss.Create(reason: dto.Reason);
        if (command is null)
          return Results.BadRequest();

        var result = await context.RequestServices.ExecuteCommandAsync<TaskCommand>(taskId, command);
        return result ? Results.Ok() : Results.BadRequest();
      }
    );

    builder.MapPost(
      "memorize",
      async (HttpContext context, [FromBody] MemorizeDto dto) =>
      {
        var command = TaskCommand.Memorize.Create(title: dto.Title);
        if (command is null)
          return Results.BadRequest();

        var taskId = Guid.CreateVersion7();
        var result = await context.RequestServices.ExecuteCommandAsync<TaskCommand>(taskId, command);
        return result ? Results.Created($"{taskRoute}/{taskId}", new { TaskId = taskId }) : Results.BadRequest();
      }
    );

    builder.MapPut(
      "{taskId}/reopen",
      static async (HttpContext context, Guid taskId) =>
      {
        var command = TaskCommand.Reopen.Create();
        if (command is null)
          return Results.BadRequest();

        var result = await context.RequestServices.ExecuteCommandAsync<TaskCommand>(taskId, command);
        return result ? Results.Ok() : Results.BadRequest();
      }
    );

    // endpoints for queries
    builder.MapGet(
      "",
      static async ([FromServices] IViewQuery<TaskQueryRequest.List, TaskQueryResponse.List> query, CancellationToken cancellationToken) =>
      {
        var request = TaskQueryRequest.List.Create(tenantId: null);
        if (request is null)
          return Results.BadRequest();

        var response = await query.ExecuteQueryAsync(request, cancellationToken);
        return Results.Ok(response);
      }
    );

    // endpoints for views
    builder.MapGet(
      "{taskId}",
      static async (HttpContext context, Guid taskId, CancellationToken cancellationToken) =>
      {
        var result = await context.RequestServices.GetProjection<TaskView.Details>(taskId, cancellationToken);
        return result is null ? Results.NotFound() : Results.Ok(result);
      }
    );

    builder.MapGet(
      "{taskId}/stream",
      static (HttpContext context, Guid taskId, CancellationToken cancellationToken) =>
      {
        var toDoStream = context.RequestServices.StreamProjection<TaskView.Details>(taskId, cancellationToken);

        return Results.ServerSentEvents(toDoStream, eventType: TaskConstants.TaskView.Details);
      }
    );

    // endpoints for debugging, ignore
    builder.MapGet(
      "events",
      static ([FromServices] IDomainEventStorage<TaskEvent> eventStore, CancellationToken cancellationToken) =>
      {
        var taskEvents = eventStore.ReadAllEventsFromStorage(cancellationToken);
        return Results.ServerSentEvents(taskEvents, eventType: TaskConstants.TaskEvent.Base);
      }
    );

    builder.MapGet(
      "{taskId}/events",
      static async (
        Guid taskId,
        [FromQuery] string? mode,
        [FromQuery] int? from,
        [FromQuery] int? to,
        [FromServices] IDomainEventStorage<TaskEvent> eventStore,
        CancellationToken cancellationToken
      ) =>
      {
        from ??= 1;
        to ??= 0;

        if (from.Value <= 0 || to.Value < 0 || (to.Value > 0 && to.Value <= from.Value))
          return Results.BadRequest();

        var taskEvents = await eventStore.ReadEventsFromStorage(taskId, from.Value, to.Value, cancellationToken);
        if (taskEvents is null)
          return Results.NotFound();

        return Results.Ok(
          mode switch
          {
            "payload" => taskEvents.Select(ev => ev.Payload).ToArray(),
            _ => taskEvents,
          }
        );
      }
    );

    return builder;
  }
}
