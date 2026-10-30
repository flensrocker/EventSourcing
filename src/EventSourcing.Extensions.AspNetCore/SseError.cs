using System.Net;
using System.Net.ServerSentEvents;

namespace Flensrocker.EventSourcing.Extensions.AspNetCore;

public record SseError
{
  public required HttpStatusCode StatusCode { get; init; }
  public string? Message { get; init; }
}

public static class SseErrorExtensions
{
  extension(SseError sseError)
  {
    public SseItem<object?> ToSseItem() => new SseItem<object?>(sseError, AspNetCoreConstants.SseErrorEventType);
  }
}
