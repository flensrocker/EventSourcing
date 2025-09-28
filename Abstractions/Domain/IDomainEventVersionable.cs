namespace OrleansPlayground.Abstractions.Domain;

public interface IDomainEventVersionable
{
  int Version { get; }
}
