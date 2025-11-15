namespace Flensrocker.EventSourcing.Orleans.Abstractions;

public interface IDomainAggegrateGrain<TCommandBase> : IGrainWithGuidKey
{
  Task<bool> ExecuteCommand(TCommandBase command);
}
