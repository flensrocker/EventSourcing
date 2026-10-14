namespace Flensrocker.EventSourcing.Abstractions;

public interface ICommandHandler<TEventBase, TCommandBase, TCommandState>
  where TCommandState : IDomainEventProjectable
{
  Task<DomainResult<TEventBase[]>> ExecuteCommandAsync(TCommandState state, TCommandBase command);
}

[GenerateSerializer]
[Alias(EventSourcingConstants.CommandResult.Base)]
public record CommandResult
{
  [Id(0)]
  public int Version { get; init; }
}

// This interface is for implementations, which load the state and then call the ICommandHandler<TEventBase, TCommandBase, TCommandState>
// Mainly used to decouple the caller like WebApi from the Orleans grains
public interface ICommandHandler<TCommandBase>
{
  /// <returns>The current version of the state</returns>
  Task<DomainResult<CommandResult>> ExecuteCommand(TCommandBase command);
}
