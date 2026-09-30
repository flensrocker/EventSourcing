using Flensrocker.EventSourcing.Orleans.Abstractions;
using Flensrocker.EventSourcing.Orleans.EntityFrameworkCore;
using Flensrocker.EventSourcing.Orleans.Server;
using Flensrocker.ToDo.Abstractions.Tasks;
using Flensrocker.ToDo.Persistence.PostgreSQL;

namespace Flensrocker.ToDo.Orleans.Server.Tasks;

// NOTE BEST PRACTICE
// If you want to aggregate the events of multiple streams into one "database row" (whatever that is):
// - implement IWhereKey<T> on your projection,
// - create a grain derived from DomainEventRouterGrain to subscribe to the domain events,
// - create a grain derived from either IUpdateDomainEventProjectionGrainWithStringKey
//   or IUpdateDomainEventProjectionGrainWithGuidKey to implement the update of the projection,
// DomainEventRouterGrain will use IWhereKey<T>.GetKey to get the target key
// and calls IUpdateDomainEventProjectionGrain.UpdateProjection on the "update grain" with the observed event.
// With this you avoid concurrent access on that one "database row".

[ImplicitStreamSubscription(TaskConstants.TaskEvent.Base)]
public class TaskViewDailyStatsRouterGrain : DomainEventRouterGrain<TaskView.DailyStats>, IGrainWithGuidKey { }

public class TaskViewDailyStatsUpdateGrain
  : EntityFrameworkCoreUpdateProjectionGrain<ToDoDbContext, TaskView.DailyStats, TaskEvent>,
    IUpdateDomainEventProjectionGrainWithStringKey<TaskView.DailyStats>
{
  public TaskViewDailyStatsUpdateGrain()
    : base(TaskConstants.TaskView.DailyStats) { }
}
