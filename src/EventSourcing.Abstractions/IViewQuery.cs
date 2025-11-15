namespace Flensrocker.EventSourcing.Abstractions;

public interface IViewQuery<TQueryRequest, TQueryResponse>
{
  Task<TQueryResponse> ExecuteQueryAsync(TQueryRequest request, CancellationToken cancellationToken);
}
