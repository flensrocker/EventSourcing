using Flensrocker.EventSourcing.Abstractions;

namespace Flensrocker.Measure.Orleans.Abstractions;

public static class MeasureExtensions
{
  extension(EventSourcingJsonSerializerOptions jsonOptions)
  {
    public EventSourcingJsonSerializerOptions AddMeasureJsonOptions() => jsonOptions.Add(MeasureJsonSerializerContext.Default);
  }
}
