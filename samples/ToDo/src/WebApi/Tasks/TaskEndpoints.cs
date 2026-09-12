using Flensrocker.EventSourcing.Abstractions;
using Flensrocker.EventSourcing.Extensions.AspNetCore;
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
    public required int Version { get; init; }
    public required Guid TaskId { get; init; }
  }

  internal static TBuilder MapTasks<TBuilder>(this TBuilder builder, string taskRoute)
    where TBuilder : IEndpointRouteBuilder
  {
    // endpoints for commands
    builder.MapPut(
      "{taskId}/complete",
      static async (HttpContext context, Guid taskId) =>
      {
        return await TaskCommand
          .Complete.Create()
          .Pipe(async command => await context.RequestServices.ExecuteCommandAsync<TaskCommand>(taskId, command))
          .ToTypedResult();
      }
    );

    builder.MapPut(
      "{taskId}/dismiss",
      static async (HttpContext context, Guid taskId, [FromBody] DismissDto dto) =>
      {
        return await TaskCommand
          .Dismiss.Create(reason: dto.Reason)
          .Pipe(async command => await context.RequestServices.ExecuteCommandAsync<TaskCommand>(taskId, command))
          .ToTypedResult();
      }
    );

    builder.MapPost(
      "memorize",
      async (HttpContext context, [FromBody] MemorizeDto dto) =>
      {
        // NOTE BEST PRACTICE
        // Use always a Guid v7 for new event streams, because they are sorted by time of creation, which is nicer for database indices.
        var taskId = Guid.CreateVersion7();

        return await TaskCommand
          .Memorize.Create(title: dto.Title)
          .Pipe(async command => await context.RequestServices.ExecuteCommandAsync<TaskCommand>(taskId, command))
          .Pipe(result => DomainResult.Success(new MemorizedDto { Version = result.Version, TaskId = taskId }))
          .ToTypedResult($"{taskRoute}/{taskId}");
      }
    );

    builder.MapPut(
      "{taskId}/reopen",
      static async (HttpContext context, Guid taskId) =>
      {
        return await TaskCommand
          .Reopen.Create()
          .Pipe(async command => await context.RequestServices.ExecuteCommandAsync<TaskCommand>(taskId, command))
          .ToTypedResult();
      }
    );

    // endpoints for queries
    builder.MapGet(
      "",
      static async ([FromServices] IViewQuery<TaskQueryRequest.List, TaskQueryResponse.List> query, CancellationToken cancellationToken) =>
      {
        return await TaskQueryRequest
          .List.Create(tenantId: null)
          .Pipe(async request => await query.ExecuteQueryAsync(request, cancellationToken))
          .ToTypedResult();
      }
    );

    // endpoints for views
    builder.MapGet(
      "{taskId}",
      static async (HttpContext context, Guid taskId, CancellationToken cancellationToken) =>
      {
        return await context.RequestServices.GetProjection<TaskView.Details>(taskId, cancellationToken).ToTypedResult();
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
