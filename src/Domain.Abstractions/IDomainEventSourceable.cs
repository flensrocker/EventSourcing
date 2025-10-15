namespace Flensrocker.Domain.Abstractions;

public interface IDomainEventSourceable<TEventBase> : IDomainEventProjectable<TEventBase>, IDomainEventVersionable
{
}
