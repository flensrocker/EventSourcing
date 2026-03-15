using Orleans.Concurrency;

namespace Flensrocker.EventSourcing.Orleans.Abstractions;

public interface IDomainEventProjectionGrain<TProjection> : IGrainWithGuidKey
{
  Task<TProjection?> GetProjection(CancellationToken cancellationToken);
  [AlwaysInterleave]
  IAsyncEnumerable<TProjection> StreamProjection(CancellationToken cancellationToken);
}
