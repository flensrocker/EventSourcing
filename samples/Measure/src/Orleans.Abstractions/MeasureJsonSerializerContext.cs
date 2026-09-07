using System.Text.Json.Serialization;

namespace Flensrocker.Measure.Orleans.Abstractions;

[JsonSerializable(typeof(DummyCommand))]
[JsonSourceGenerationOptions(AllowOutOfOrderMetadataProperties = true)]
public partial class MeasureJsonSerializerContext : JsonSerializerContext { }
