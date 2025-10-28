using Orleans;

namespace Flensrocker.Orleans.Domain.Abstractions;

public interface IDomainAggegrateGrain<TCommandBase> : IGrainWithGuidKey
{
  Task<bool> ExecuteCommand(TCommandBase command);
}
