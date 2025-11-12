using Flensrocker.Domain.Abstractions;
using Flensrocker.ToDo.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Flensrocker.ToDo.Persistence.PostgreSQL.Queries;

internal class ListToDosViewQuery(IDbContextFactory<ToDoDbContext> dbContextFactory)
  : IViewQuery<ToDoQuery.ListToDosRequest, ToDoQuery.ListToDosResponse>
{
  private readonly IDbContextFactory<ToDoDbContext> _dbContextFactory = dbContextFactory;

  public async Task<ToDoQuery.ListToDosResponse> ExecuteQueryAsync(ToDoQuery.ListToDosRequest request, CancellationToken cancellationToken)
  {
    using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

    var listItems = await dbContext.ListItem
      .Where(e => e.TenantId == request.TenantId)
      .OrderBy(e => e.Status)
      .ThenByDescending(e => e.CreatedAt)
      .ToArrayAsync(cancellationToken);

    return new ToDoQuery.ListToDosResponse
    {
      ToDoItemList = listItems,
    };
  }
}
