namespace Flensrocker.Domain.Orleans.Abstractions;

public interface IDomainEventProjectionGrain<TProjection> : IGrainWithGuidKey
{
  Task<TProjection?> GetProjection(CancellationToken cancellationToken);
}
