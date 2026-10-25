using Flensrocker.EventSourcing.Abstractions;
using Flensrocker.ToDo.Abstractions.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Flensrocker.ToDo.Persistence.PostgreSQL.Tasks;

internal class DailyStatsTasksViewQuery(IDbContextFactory<ToDoDbContext> dbContextFactory)
  : IViewQuery<TaskQueryRequest.DailyStats, TaskQueryResponse.DailyStats>
{
  private readonly IDbContextFactory<ToDoDbContext> _dbContextFactory = dbContextFactory;

  public async Task<DomainResult<TaskQueryResponse.DailyStats>> ExecuteQueryAsync(
    TaskQueryRequest.DailyStats request,
    CancellationToken cancellationToken
  )
  {
    using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

    var itemList = await dbContext
      .TaskDailyStats.AsNoTracking()
      .Where(e =>
        (!request.FromDate.HasValue || e.Day >= request.FromDate.Value)
        && (!request.ToDate.HasValue || e.Day <= request.ToDate.Value)
        && e.TenantId == request.TenantId
        && e.UserId == request.UserId
      )
      .OrderBy(e => e.Day)
      .ThenBy(e => e.TenantId)
      .ThenBy(e => e.UserId)
      .Select(e => new TaskQueryResponse.DailyStats.DailyStatsItem
      {
        Day = e.Day,
        UserId = e.UserId,
        CountOpened = e.CountOpened,
        CountCompleted = e.CountCompleted,
        CountDismissed = e.CountDismissed,
      })
      .ToArrayAsync(cancellationToken);

    return new TaskQueryResponse.DailyStats { ItemList = itemList };
  }
}
