namespace Flensrocker.EventSourcing.Abstractions;

public static class EventSourcingConstants
{
  public const string TraceSourceName = $"{nameof(Flensrocker)}.{nameof(EventSourcing)}";
  public const string TraceVersion = "1.0.0";

  public const string MeterName = $"{nameof(Flensrocker)}.{nameof(EventSourcing)}";
  public const string MeterVersion = "1.0.0";
}
