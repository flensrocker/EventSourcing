using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Flensrocker.EventSourcing.Abstractions;

public class DomainEventPublisherMetrics
{
  private readonly Counter<long> _publishedMessages;
  private readonly Counter<long> _publishedDomainEvents;
  private readonly Histogram<double> _publishToStreamDurations;

  public DomainEventPublisherMetrics(IMeterFactory meterFactory)
  {
    var meter = meterFactory.Create(EventSourcingConstants.MeterName, EventSourcingConstants.MeterVersion);

    _publishedMessages = meter.CreateCounter<long>("messages-published", "Messages", "The number of messages published");
    _publishedDomainEvents = meter.CreateCounter<long>("domainevents-published", "Domain-Events", "The number of domain-events published");
    _publishToStreamDurations = meter.CreateHistogram<double>("publish-duration", "ms", "The duration of publishing");
  }

  public void MessagePublished() => _publishedMessages.Add(1L);

  public void DomainEventPublished() => _publishedDomainEvents.Add(1L);

  public void PublishToStreamDuration(long startingTimestamp) =>
    _publishToStreamDurations.Record(Stopwatch.GetElapsedTime(startingTimestamp).TotalMilliseconds);
}
