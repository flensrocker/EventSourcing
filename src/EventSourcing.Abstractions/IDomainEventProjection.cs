namespace Flensrocker.EventSourcing.Abstractions;

public interface IDomainEventProjection<TProjection>
{
  Task<DomainResult<TProjection>> GetProjection(CancellationToken cancellationToken);
  IAsyncEnumerable<TProjection> StreamProjection(CancellationToken cancellationToken);
}
