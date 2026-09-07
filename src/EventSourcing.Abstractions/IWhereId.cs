using System.Linq.Expressions;

namespace Flensrocker.EventSourcing.Abstractions;

public interface IWhereId<T>
{
  static abstract Expression<Func<T, bool>> GetWhereId(Guid id);
}
