namespace OrleansPlayground.Abstractions.Domain;

public interface IDomainEventSourceable<TEventBase> : IDomainEventProjection<TEventBase>
{
  int Version { get; }
}
