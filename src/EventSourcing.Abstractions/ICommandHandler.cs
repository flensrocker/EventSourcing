namespace Flensrocker.EventSourcing.Abstractions;

public interface ICommandHandler<TEventBase, TCommandBase, TCommandState>
  where TCommandState : IDomainEventProjectable
{
  ValueTask<TEventBase[]?> ExecuteCommandAsync(TCommandState state, TCommandBase command);
}
