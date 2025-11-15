using System.Linq.Expressions;

namespace Flensrocker.EventSourcing.Abstractions;

public interface IWhereId<T>
{
  abstract static Expression<Func<T, bool>> GetWhereId(Guid id);
}
