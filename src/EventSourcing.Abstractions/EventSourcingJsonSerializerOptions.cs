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
  private readonly Dictionary<string, Type> _nameToType = new();
  private readonly Dictionary<Type, string> _typeToName = new();

  public static EventSourcingJsonSerializerOptions Default { get; } = new();

  public JsonSerializerOptions Options { get; } = new JsonSerializerOptions
  {
    AllowOutOfOrderMetadataProperties = true,
    PropertyNamingPolicy = null,
  };

  public EventSourcingJsonSerializerOptions()
  {
    Add(EventSourcingJsonSerializerContext.Default);
    Add<PersistentProjectionEvent>(PersistentProjectionConstants.PersistentProjectionEvent.Base);
  }

  public EventSourcingJsonSerializerOptions Add(JsonSerializerContext context)
  {
    if (Options.IsReadOnly)
      throw new InvalidOperationException($"{nameof(EventSourcingJsonSerializerOptions)} is already readonly");

    Options.WithTypeResolversOf(context);
    return this;
  }

  public EventSourcingJsonSerializerOptions Add<T>(string typeName)
  {
    if (Options.IsReadOnly)
      throw new InvalidOperationException($"{nameof(EventSourcingJsonSerializerOptions)} is already readonly");

    _nameToType.TryAdd(typeName, typeof(T));
    _typeToName.TryAdd(typeof(T), typeName);
    return this;
  }

  public Type? GetTypeFromName(string typeName)
  {
    if (string.IsNullOrWhiteSpace(typeName))
      return null;

    return _nameToType.TryGetValue(typeName, out var type)
      ? type
      : null;
  }

  public string GetTypeName(Type? type)
  {
    return type is not null && _typeToName.TryGetValue(type, out var typeName)
      ? typeName
      : "";
  }

  public EventSourcingJsonSerializerOptions MakeReadOnly()
  {
    if (!Options.IsReadOnly)
    {
      Options.MakeReadOnly();
    }

    return this;
  }
}
