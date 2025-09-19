namespace OrleansPlayground.Abstractions.Domain;

public interface IEventSourceable<TEventBase>
{
  void ApplyEvents(params IEnumerable<DomainEvent<TEventBase>> @event);
}
