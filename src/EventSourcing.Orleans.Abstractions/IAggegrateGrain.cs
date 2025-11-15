namespace Flensrocker.EventSourcing.Orleans.Abstractions;

public interface IAggegrateGrain<TCommandBase> : IGrainWithGuidKey
{
  Task<bool> ExecuteCommand(TCommandBase command);
}
