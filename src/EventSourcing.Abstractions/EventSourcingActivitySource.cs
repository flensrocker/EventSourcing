using System.Diagnostics;

namespace Flensrocker.EventSourcing.Abstractions;

public sealed class EventSourcingActivitySource
{
  private readonly ActivitySource _source = new ActivitySource(EventSourcingConstants.TraceSourceName, EventSourcingConstants.TraceVersion);

  public ActivitySource Source
  {
    get => _source;
  }
}
