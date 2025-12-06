using System.Diagnostics;
using System.Diagnostics.Metrics;
using Flensrocker.EventSourcing.Abstractions;

namespace Flensrocker.EventSourcing.Persistence.PostgreSQL;

public class PostgreSQLMessageSubscriberMetrics
{
  private readonly Counter<long> _receivedMessages;
  private readonly Counter<long> _receivedInserts;
  private readonly Counter<long> _sentInserts;
  private readonly Histogram<double> _publishDurations;

  public PostgreSQLMessageSubscriberMetrics(IMeterFactory meterFactory)
  {
    var meter = meterFactory.Create(EventSourcingConstants.MeterName, EventSourcingConstants.MeterVersion);

    _receivedMessages = meter.CreateCounter<long>("messages-received", "Messages", "The number of messages received from PostgreSQL");
    _receivedInserts = meter.CreateCounter<long>("inserts-received", "Messages", "The number of insert-messages received from PostgreSQL");
    _sentInserts = meter.CreateCounter<long>("inserts-sent", "Messages", "The number of insert-messages sent to publisher");
    _publishDurations = meter.CreateHistogram<double>("publish-duration", "ms", "The duration of publishing the messages received from PostgreSQL");
  }

  public void MessageReceived()
    => _receivedMessages.Add(1L);

  public void InsertReceived()
    => _receivedInserts.Add(1L);

  public void InsertSent()
    => _sentInserts.Add(1L);

  public void PublishDuration(long startingTimestamp)
    => _publishDurations.Record(Stopwatch.GetElapsedTime(startingTimestamp).TotalMilliseconds);
}
