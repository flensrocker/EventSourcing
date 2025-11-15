namespace Flensrocker.EventSourcing.Abstractions;

public interface IDomainEventVersionable
{
  int Version { get; }
}
