using System.Linq.Expressions;

namespace Flensrocker.EventSourcing.Abstractions;

public interface IWhereKey<T>
{
  static abstract object GetKey(DomainEvent domainEvent);

  static abstract Expression<Func<T, bool>> GetWhereKey(DomainEvent domainEvent);
}
