using System.Diagnostics.Metrics;
using Flensrocker.EventSourcing.Abstractions;

namespace Flensrocker.EventSourcing.Orleans.EntityFrameworkCore;

public class OrleansEntityFrameworkCoreProjectionMetrics
{
  private readonly Counter<long> _receivedProjectionMessages;
  private readonly Counter<long> _skippedProjectionMessages;
  private readonly Counter<long> _savedProjectionMessages;
  private readonly Counter<long> _erroredProjectionStreams;

  public OrleansEntityFrameworkCoreProjectionMetrics(IMeterFactory meterFactory)
  {
    var meter = meterFactory.Create(EventSourcingConstants.MeterName, EventSourcingConstants.MeterVersion);

    _receivedProjectionMessages = meter.CreateCounter<long>(
      "projection-messages-received",
      "Messages",
      "The number of messages received from Orleans"
    );
    _skippedProjectionMessages = meter.CreateCounter<long>(
      "projection-messages-skipped",
      "Messages",
      "The number of messages skipped because of too old stream version"
    );
    _savedProjectionMessages = meter.CreateCounter<long>(
      "projection-messages-saved",
      "Messages",
      "The number of messages saved to PostgreSQL"
    );
    _erroredProjectionStreams = meter.CreateCounter<long>(
      "projection-stream-errored",
      "Errors",
      "The number of errors from the Orleans Stream"
    );
  }

  public void ProjectionMessagesReceived() => _receivedProjectionMessages.Add(1L);

  public void ProjectionMessagesSkipped() => _skippedProjectionMessages.Add(1L);

  public void ProjectionMessagesSaved() => _savedProjectionMessages.Add(1L);

  public void ProjectionStreamErrored() => _erroredProjectionStreams.Add(1L);
}
