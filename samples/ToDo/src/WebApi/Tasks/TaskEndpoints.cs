using Flensrocker.EventSourcing.Abstractions;
using Flensrocker.ToDo.Abstractions.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Flensrocker.ToDo.WebApi.Tasks;

public static class TaskEndpoints
{
  public record DismissDto
  {
    public string? Reason { get; init; }
  }

  public record MemorizeDto
  {
    public string? Title { get; init; }
  }

  public record MemorizedDto
  {
    public required Guid TaskId { get; init; }
  }

  internal static TBuilder MapTasks<TBuilder>(this TBuilder builder, string taskRoute)
    where TBuilder : IEndpointRouteBuilder
  {
    // endpoints for commands
    builder.MapPut(
      "{taskId}/complete",
      static async Task<Results<Ok, BadRequest>> (HttpContext context, Guid taskId) =>
      {
        var command = TaskCommand.Complete.Create();
        if (command is null)
          return TypedResults.BadRequest();

        var result = await context.RequestServices.ExecuteCommandAsync<TaskCommand>(taskId, command);
        return result ? TypedResults.Ok() : TypedResults.BadRequest();
      }
    );

    builder.MapPut(
      "{taskId}/dismiss",
      static async Task<Results<Ok, BadRequest>> (HttpContext context, Guid taskId, [FromBody] DismissDto dto) =>
      {
        var command = TaskCommand.Dismiss.Create(reason: dto.Reason);
        if (command is null)
          return TypedResults.BadRequest();

        var result = await context.RequestServices.ExecuteCommandAsync<TaskCommand>(taskId, command);
        return result ? TypedResults.Ok() : TypedResults.BadRequest();
      }
    );

    builder.MapPost(
      "memorize",
      async Task<Results<Created<MemorizedDto>, BadRequest, ValidationProblem>> (HttpContext context, [FromBody] MemorizeDto dto) =>
      {
        var commandResult = TaskCommand.Memorize.Create(title: dto.Title);
        if (commandResult.IsSuccess<TaskCommand.Memorize>(out var command))
        {
          // NOTE BEST PRACTICE
          // Use always a Guid v7 for new event streams, because they are sorted by time of creation, which is nicer for database indices.
          var taskId = Guid.CreateVersion7();
          var result = await context.RequestServices.ExecuteCommandAsync<TaskCommand>(taskId, command);
          if (result)
            return TypedResults.Created($"{taskRoute}/{taskId}", new MemorizedDto { TaskId = taskId });
        }
        // TODO create DomainResult extensions for converting errors into TypedResults
        else if (commandResult is DomainResult.ValidationProblem validationProblem)
        {
          return TypedResults.ValidationProblem(
            errors: validationProblem.Errors,
            detail: validationProblem.Detail,
            title: validationProblem.Title
          );
        }

        return TypedResults.BadRequest();
      }
    );

    builder.MapPut(
      "{taskId}/reopen",
      static async Task<Results<Ok, BadRequest>> (HttpContext context, Guid taskId) =>
      {
        var command = TaskCommand.Reopen.Create();
        if (command is null)
          return TypedResults.BadRequest();

        var result = await context.RequestServices.ExecuteCommandAsync<TaskCommand>(taskId, command);
        return result ? TypedResults.Ok() : TypedResults.BadRequest();
      }
    );

    // endpoints for queries
    builder.MapGet(
      "",
      static async Task<Results<Ok<TaskQueryResponse.List>, BadRequest>> (
        [FromServices] IViewQuery<TaskQueryRequest.List, TaskQueryResponse.List> query,
        CancellationToken cancellationToken
      ) =>
      {
        var request = TaskQueryRequest.List.Create(tenantId: null);
        if (request is null)
          return TypedResults.BadRequest();

        var response = await query.ExecuteQueryAsync(request, cancellationToken);
        return TypedResults.Ok(response);
      }
    );

    // endpoints for views
    builder.MapGet(
      "{taskId}",
      static async Task<Results<Ok<TaskView.Details>, NotFound>> (HttpContext context, Guid taskId, CancellationToken cancellationToken) =>
      {
        var result = await context.RequestServices.GetProjection<TaskView.Details>(taskId, cancellationToken);
        return result is null ? TypedResults.NotFound() : TypedResults.Ok(result);
      }
    );

    builder.MapGet(
      "{taskId}/stream",
      static (HttpContext context, Guid taskId, CancellationToken cancellationToken) =>
      {
        var toDoStream = context.RequestServices.StreamProjection<TaskView.Details>(taskId, cancellationToken);
        return TypedResults.ServerSentEvents(toDoStream, eventType: TaskConstants.TaskView.Details);
      }
    );

    // endpoints for debugging, ignore
    builder.MapGet(
      "events",
      static ([FromServices] IDomainEventStorage<TaskEvent> eventStore, CancellationToken cancellationToken) =>
      {
        var taskEvents = eventStore.ReadAllEventsFromStorage(cancellationToken);
        return TypedResults.ServerSentEvents(taskEvents, eventType: TaskConstants.TaskEvent.Base);
      }
    );

    builder.MapGet(
      "{taskId}/events",
      static async Task<Results<Ok<IEnumerable<object>>, Ok<IEnumerable<DomainEvent>>, BadRequest, NotFound>> (
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
          return TypedResults.BadRequest();

        var taskEvents = await eventStore.ReadEventsFromStorage(taskId, from.Value, to.Value, cancellationToken);
        if (taskEvents is null)
          return TypedResults.NotFound();

        return mode switch
        {
          "payload" => TypedResults.Ok(taskEvents.Select(ev => ev.Payload)),
          _ => TypedResults.Ok(taskEvents.AsEnumerable()),
        };
      }
    );

    return builder;
  }
}
