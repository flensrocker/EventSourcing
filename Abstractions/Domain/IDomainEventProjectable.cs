namespace OrleansPlayground.Abstractions.Domain;

public interface IDomainEventProjectable<TEventBase>
{
  void Apply(DomainEvent<TEventBase> @event);

  void ApplyEvents(params IEnumerable<DomainEvent<TEventBase>> events)
  {
    foreach (var @event in events)
      Apply(@event);
  }
}
