namespace OrleansPlayground.Abstractions.Domain;

public interface IState<TEventBase>
{
  void ApplyEvents(params IEnumerable<TEventBase> @event);
}
