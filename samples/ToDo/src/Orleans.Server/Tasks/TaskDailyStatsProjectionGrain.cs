using Flensrocker.EventSourcing.Orleans.EntityFrameworkCore;
using Flensrocker.ToDo.Abstractions.Tasks;
using Flensrocker.ToDo.Persistence.PostgreSQL;

namespace Flensrocker.ToDo.Orleans.Server.Tasks;

[ImplicitStreamSubscription(TaskConstants.TaskEvent.Base)]
public class TaskDailyStatsProjectionGrain : EntityFrameworkCoreProjectionGrain<ToDoDbContext, TaskView.DailyStats, TaskEvent>
{
  public TaskDailyStatsProjectionGrain()
    : base(TaskConstants.TaskView.DailyStats) { }
}
