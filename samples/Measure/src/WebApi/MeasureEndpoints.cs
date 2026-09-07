using Flensrocker.EventSourcing.Abstractions;
using Flensrocker.Measure.Orleans.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Flensrocker.Measure.WebApi;

public static class MeasureEndpoints
{
  public record DummyDto
  {
    public string? Content { get; init; }
  }

  private static readonly Guid[] _grainIds = Enumerable
    .Range(0, 50)
    .Select(_ => Guid.NewGuid())
    .ToArray();

  private static Guid GetRandomGrainId() =>
    _grainIds[DateTime.UtcNow.Microsecond % _grainIds.Length];

  public static IEndpointRouteBuilder MapMeasure(this IEndpointRouteBuilder builder)
  {
    builder.MapPost(
      "/api/measure/webapi-only",
      static async ([FromBody] DummyDto dto) =>
      {
        return Results.Ok();
      }
    );

    builder.MapPost(
      "/api/measure/webapi-grain",
      static async ([FromBody] DummyDto dto, [FromServices] IClusterClient client) =>
      {
        var command = new DummyCommand { Content = dto.Content ?? "" };
        var grainId = GetRandomGrainId();
        var dummy = client.GetGrain<IDummyGrain>(grainId);
        await dummy.ExecuteNoop(command);
        return Results.Ok();
      }
    );

    return builder;
  }
}
