using System.Text.Json;
using System.Text.Json.Serialization;
using Flensrocker.EventSourcing.Abstractions.PersistentProjection;

namespace Flensrocker.EventSourcing.Abstractions;

[JsonSerializable(typeof(DomainEvent))]
[JsonSerializable(typeof(PersistentProjectionEvent))]
[JsonSerializable(typeof(PersistentProjectionEvent.ProjectionRegistered))]
[JsonSerializable(typeof(PersistentProjectionEvent.ProjectionRebuilt))]
[JsonSourceGenerationOptions(AllowOutOfOrderMetadataProperties = true)]
internal partial class EventSourcingJsonSerializerContext : JsonSerializerContext
{
}

public class EventSourcingJsonSerializerOptions
{
  public static EventSourcingJsonSerializerOptions Default { get; } = new();

  public JsonSerializerOptions Options { get; } = new JsonSerializerOptions
  {
    AllowOutOfOrderMetadataProperties = true,
    PropertyNamingPolicy = null,
  };

  public EventSourcingJsonSerializerOptions()
  {
    Add(EventSourcingJsonSerializerContext.Default);
  }

  public EventSourcingJsonSerializerOptions Add(JsonSerializerContext context)
  {
    Options.WithTypeResolversOf(context);
    return this;
  }

  public static implicit operator JsonSerializerOptions(EventSourcingJsonSerializerOptions options)
    => new JsonSerializerOptions(options.Options);
}
