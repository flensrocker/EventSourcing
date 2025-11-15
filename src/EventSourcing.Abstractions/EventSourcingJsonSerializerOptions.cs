using System.Text.Json;
using System.Text.Json.Serialization;

namespace Flensrocker.EventSourcing.Abstractions;

public interface ISupportJsonSerialization
{
  bool Supports(Type type);
}

[JsonSerializable(typeof(DomainEvent))]
[JsonSourceGenerationOptions(AllowOutOfOrderMetadataProperties = true)]
internal partial class EventSourcingJsonSerializerContext : JsonSerializerContext, ISupportJsonSerialization
{
  public bool Supports(Type type)
    => type == typeof(DomainEvent);
}

public class EventSourcingJsonSerializerOptions
{
  public static EventSourcingJsonSerializerOptions Default { get; } = new();

  private readonly List<Func<Type, bool>> _supports = new();

  public JsonSerializerOptions Options { get; } = new JsonSerializerOptions
  {
    AllowOutOfOrderMetadataProperties = true,
    PropertyNamingPolicy = null,
  };

  public EventSourcingJsonSerializerOptions()
  {
    Add(EventSourcingJsonSerializerContext.Default);
  }

  public EventSourcingJsonSerializerOptions Add<T>(T context)
    where T : JsonSerializerContext, ISupportJsonSerialization
  {
    return Add(context.Supports, context);
  }

  public EventSourcingJsonSerializerOptions Add(Func<Type, bool> supports, JsonSerializerContext context)
  {
    _supports.Add(supports);
    Options.WithTypeResolversOf(context);
    return this;
  }

  public bool Supports(Type type)
  {
    return _supports.Any(supports => supports(type));
  }

  public static implicit operator JsonSerializerOptions(EventSourcingJsonSerializerOptions options)
    => new JsonSerializerOptions(options.Options);
}
