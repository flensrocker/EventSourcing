using Flensrocker.ToDo.Abstractions.Views;
using Flensrocker.ToDo.Orleans.Abstractions;
using Flensrocker.ToDo.Persistence.PostgreSQL;
using Microsoft.EntityFrameworkCore;

namespace Flensrocker.ToDo.Orleans.Server;

public class ToDoListViewGrain(IDbContextFactory<ToDoDbContext> dbContextFactory) : Grain, IToDoListViewGrain
{
  private readonly IDbContextFactory<ToDoDbContext> _dbContextFactory = dbContextFactory;

  public async Task<ToDoListItemView[]> GetList(CancellationToken cancellationToken)
  {
    var tenantId = this.GetPrimaryKey() == Guid.Empty ? (Guid?)null : this.GetPrimaryKey();

    using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

    var listItems = await dbContext.ListItem
      .Where(e => e.TenantId == tenantId)
      .OrderBy(e => e.Status)
      .ThenByDescending(e => e.CreatedAt)
      .ToArrayAsync(cancellationToken);

    return listItems;
  }
}
