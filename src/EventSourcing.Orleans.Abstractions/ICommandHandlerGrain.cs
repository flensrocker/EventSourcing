namespace Flensrocker.EventSourcing.Orleans.Abstractions;

public interface ICommandHandlerGrain<TCommandBase> : IGrainWithGuidKey
{
  Task<bool> ExecuteCommand(TCommandBase command);
}
