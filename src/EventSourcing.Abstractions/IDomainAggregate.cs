namespace Flensrocker.EventSourcing.Abstractions;

public interface IDomainAggregate<TCommandBase, TEventBase>
{
  ValueTask<TEventBase[]?> ExecuteCommandAsync(TCommandBase command);
}
