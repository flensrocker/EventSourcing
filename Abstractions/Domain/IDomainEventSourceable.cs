namespace OrleansPlayground.Abstractions.Domain;

public interface IDomainEventSourceable<TEventBase>
{
  void ApplyEvents(params IEnumerable<DomainEvent<TEventBase>> @event);
}
