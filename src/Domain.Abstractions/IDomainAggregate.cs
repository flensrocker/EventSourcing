namespace Flensrocker.Domain.Abstractions;

public interface IDomainAggregate<TCommandBase, TEventBase>
{
  TEventBase[]? ExecuteCommand(TCommandBase command);
}
