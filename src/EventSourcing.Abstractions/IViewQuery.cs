namespace Flensrocker.EventSourcing.Abstractions;

public interface IViewQuery<TQueryRequest, TQueryResponse>
{
  ValueTask<TQueryResponse> ExecuteQueryAsync(TQueryRequest request, CancellationToken cancellationToken);
}
