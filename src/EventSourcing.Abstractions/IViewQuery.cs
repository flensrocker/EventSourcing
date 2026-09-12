namespace Flensrocker.EventSourcing.Abstractions;

public interface IViewQuery<TQueryRequest, TQueryResponse>
{
  Task<DomainResult<TQueryResponse>> ExecuteQueryAsync(TQueryRequest request, CancellationToken cancellationToken);
}
