namespace Flensrocker.Domain.Abstractions;

public interface IDomainAggregate<TCommandBase, TEventBase>
{
  ValueTask<TEventBase[]?> ExecuteCommandAsync(TCommandBase command);
}
