namespace Flensrocker.EventSourcing.Abstractions;

public interface IAggregate<TCommandBase, TEventBase>
{
  ValueTask<TEventBase[]?> ExecuteCommandAsync(TCommandBase command);
}
