using System.Linq.Expressions;

namespace Flensrocker.EventSourcing.Abstractions;

public interface IWhereKey<T>
{
  // TODO change return type to union(string, Guid) in C# 15/.NET 11
  static abstract object GetKey(DomainEvent domainEvent);

  static abstract Expression<Func<T, bool>> GetWhereKey(DomainEvent domainEvent);
}
