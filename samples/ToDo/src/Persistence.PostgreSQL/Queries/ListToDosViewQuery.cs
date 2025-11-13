using Flensrocker.Domain.Abstractions;
using Flensrocker.ToDo.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Flensrocker.ToDo.Persistence.PostgreSQL.Queries;

internal class ListToDosViewQuery(IDbContextFactory<ToDoDbContext> dbContextFactory)
  : IViewQuery<ToDoQueryRequest.ListToDos, ToDoQueryResponse.ListToDos>
{
  private readonly IDbContextFactory<ToDoDbContext> _dbContextFactory = dbContextFactory;

  public async Task<ToDoQueryResponse.ListToDos> ExecuteQueryAsync(ToDoQueryRequest.ListToDos request, CancellationToken cancellationToken)
  {
    using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

    var listItems = await dbContext.ListItem
      .Where(e => e.TenantId == request.TenantId)
      .OrderBy(e => e.Status)
      .ThenByDescending(e => e.CreatedAt)
      .Select(e => new ToDoQueryResponse.ListToDos.ToDoItem
      {
        ToDoId = e.ToDoId,
        Version = e.Version,
        CreatedAt = e.CreatedAt,
        Title = e.Title,
        Status = e.Status,
      })
      .ToArrayAsync(cancellationToken);

    return new ToDoQueryResponse.ListToDos
    {
      ToDoItemList = listItems,
    };
  }
}
