using System.Linq.Expressions;

namespace Flensrocker.Domain.Abstractions;

public interface IWhereId<T>
{
  abstract static Expression<Func<T, bool>> GetWhereId(Guid id);
}
