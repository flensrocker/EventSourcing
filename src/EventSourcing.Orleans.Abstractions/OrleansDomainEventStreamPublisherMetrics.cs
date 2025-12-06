using System.Diagnostics;
using System.Diagnostics.Metrics;
using Flensrocker.EventSourcing.Abstractions;

namespace Flensrocker.EventSourcing.Orleans.Abstractions;

public class OrleansDomainEventStreamPublisherMetrics
{
  private readonly Counter<long> _publishedMessages;
  private readonly Counter<long> _publishedDomainEvents;
  private readonly Histogram<double> _publishToStreamDurations;

  public OrleansDomainEventStreamPublisherMetrics(IMeterFactory meterFactory)
  {
    var meter = meterFactory.Create(EventSourcingConstants.MeterName, EventSourcingConstants.MeterVersion);

    _publishedMessages = meter.CreateCounter<long>("messages-published", "Messages", "The number of messages published to Orleans");
    _publishedDomainEvents = meter.CreateCounter<long>("domainevents-published", "Domain-Events", "The number of domain-events published to Orleans");
    _publishToStreamDurations = meter.CreateHistogram<double>("publish-to-stream-duration", "ms", "The duration of publishing to the Orleans Stream");
  }

  public void MessagePublished()
    => _publishedMessages.Add(1L);

  public void DomainEventPublished()
    => _publishedDomainEvents.Add(1L);

  public void PublishToStreamDuration(long startingTimestamp)
    => _publishToStreamDurations.Record(Stopwatch.GetElapsedTime(startingTimestamp).TotalMilliseconds);
}
