namespace OrleansPlayground.Abstractions.Domain;

public interface IDomainEventSourceable<TEventBase> : IDomainEventProjectable<TEventBase>, IDomainEventVersionable
{
}
