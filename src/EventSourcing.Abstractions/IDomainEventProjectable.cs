namespace Flensrocker.EventSourcing.Abstractions;

public interface IDomainEventProjectable
{
  void Apply(DomainEvent @event);

  void ApplyEvents(params IReadOnlyList<DomainEvent> events)
  {
    foreach (var @event in events)
      Apply(@event);
  }
}
