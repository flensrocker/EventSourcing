using Flensrocker.EventSourcing.Abstractions;
using Flensrocker.ToDo.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Flensrocker.ToDo.Persistence.PostgreSQL.Queries;

internal class ListTasksViewQuery(IDbContextFactory<ToDoDbContext> dbContextFactory)
  : IViewQuery<TaskQueryRequest.List, TaskQueryResponse.List>
{
  private readonly IDbContextFactory<ToDoDbContext> _dbContextFactory = dbContextFactory;

  public async ValueTask<TaskQueryResponse.List> ExecuteQueryAsync(TaskQueryRequest.List request, CancellationToken cancellationToken)
  {
    using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

    var listItems = await dbContext
      .TaskListItem.AsNoTracking()
      .Where(e => e.TenantId == request.TenantId)
      .OrderBy(e => e.Status)
      .ThenByDescending(e => e.CreatedAt)
      .Select(e => new TaskQueryResponse.List.Item
      {
        TaskId = e.TaskId,
        Version = e.Version,
        CreatedAt = e.CreatedAt,
        Title = e.Title,
        Status = e.Status,
      })
      .ToArrayAsync(cancellationToken);

    return new TaskQueryResponse.List { ItemList = listItems };
  }
}
