namespace Flensrocker.EventSourcing.Abstractions;

public interface IDomainEventProjection<TProjection>
{
  Task<DomainResult<TProjection>> GetProjection(CancellationToken cancellationToken);
  IAsyncEnumerable<DomainResult<TProjection>> StreamProjection(CancellationToken cancellationToken);
}
