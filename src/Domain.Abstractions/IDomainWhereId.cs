using System.Linq.Expressions;

namespace Flensrocker.Domain.Abstractions;

public interface IDomainWhereId<T>
{
  abstract static Expression<Func<T, bool>> GetWhereId(Guid id);
}
