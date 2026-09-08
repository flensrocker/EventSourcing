using Flensrocker.EventSourcing.Abstractions;

namespace Flensrocker.EventSourcing.Orleans.Abstractions;

public interface ICommandHandlerGrain<TCommandBase> : ICommandHandler<TCommandBase>, IGrainWithGuidKey { }
