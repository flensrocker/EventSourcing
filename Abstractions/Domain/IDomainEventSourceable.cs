namespace OrleansPlayground.Abstractions.Domain;

public interface IDomainEventSourceable<TEventBase>
{
  int Version { get; }

  void Apply(DomainEvent<TEventBase> @event);

  void ApplyEvents(params IEnumerable<DomainEvent<TEventBase>> events)
  {
    foreach (var @event in events)
      Apply(@event);
  }
}
