using System.Linq.Expressions;

namespace Flensrocker.EventSourcing.Abstractions;

public interface IWhereKey<T>
{
  static abstract Expression<Func<T, bool>> GetWhereKey(DomainEventMetadata metadata);
}
