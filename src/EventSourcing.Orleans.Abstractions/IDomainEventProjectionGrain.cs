namespace Flensrocker.EventSourcing.Orleans.Abstractions;

public interface IDomainEventProjectionGrain<TProjection> : IGrainWithGuidKey
{
  Task<TProjection?> GetProjection(CancellationToken cancellationToken);
}
