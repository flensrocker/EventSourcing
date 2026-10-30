using Flensrocker.EventSourcing.Abstractions;

namespace Flensrocker.EventSourcing.Extensions.AspNetCore;

public static class AspNetCoreExtensions
{
  extension(EventSourcingJsonSerializerOptions jsonOptions)
  {
    public EventSourcingJsonSerializerOptions AddAspNetCoreJsonOptions() => jsonOptions.Add(AspNetCoreJsonSerializerContext.Default);
  }
}
