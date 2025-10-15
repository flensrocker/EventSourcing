namespace Flensrocker.Domain.Abstractions;

public interface IDomainEventVersionable
{
  int Version { get; }
}
