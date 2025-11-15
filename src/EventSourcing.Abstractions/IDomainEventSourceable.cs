namespace Flensrocker.EventSourcing.Abstractions;

public interface IDomainEventSourceable<TEventBase> : IDomainEventProjectable<TEventBase>, IDomainEventVersionable
{
}
