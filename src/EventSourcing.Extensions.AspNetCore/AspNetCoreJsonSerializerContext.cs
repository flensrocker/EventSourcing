using System.Text.Json.Serialization;

namespace Flensrocker.EventSourcing.Extensions.AspNetCore;

[JsonSerializable(typeof(SseError))]
[JsonSourceGenerationOptions(AllowOutOfOrderMetadataProperties = true)]
public partial class AspNetCoreJsonSerializerContext : JsonSerializerContext { }
