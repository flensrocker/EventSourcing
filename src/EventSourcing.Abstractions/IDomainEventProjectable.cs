namespace Flensrocker.EventSourcing.Abstractions;

public interface IDomainEventProjectable<TEventBase>
{
  void Apply(DomainEvent @event);

  void ApplyEvents(params IEnumerable<DomainEvent> events)
  {
    foreach (var @event in events)
      Apply(@event);
  }
}
