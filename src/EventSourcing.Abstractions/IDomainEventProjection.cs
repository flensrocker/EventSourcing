namespace Flensrocker.EventSourcing.Abstractions;

public interface IDomainEventProjection<TProjection>
{
  Task<TProjection?> GetProjection(CancellationToken cancellationToken);
  IAsyncEnumerable<TProjection> StreamProjection(CancellationToken cancellationToken);
}
