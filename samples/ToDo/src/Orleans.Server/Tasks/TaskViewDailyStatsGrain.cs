using Flensrocker.EventSourcing.Abstractions;
using Flensrocker.EventSourcing.Orleans.Abstractions;
using Flensrocker.EventSourcing.Orleans.EntityFrameworkCore;
using Flensrocker.EventSourcing.Orleans.Server;
using Flensrocker.ToDo.Abstractions.Tasks;
using Flensrocker.ToDo.Persistence.PostgreSQL;

namespace Flensrocker.ToDo.Orleans.Server.Tasks;

// NOTE BEST PRACTICE
// If you want to aggregate the events of multiple streams into one "database row" (whatever that is):
// - subscribe to the stream of the domain events,
// - calculate the target key from that data,
// - and call the "update grain" with the events.
// With this you avoid concurrent access on that one "database row".
// The DomainEventRouterGrain helps you, it will call the IUpdateDomainEventProjectionGrain.UpdateProjection method of the target.
// Just return the target key as string or Guid from the GetTargetKey override.
// And implement one grain derived from either IUpdateDomainEventProjectionGrainWithStringKey or IUpdateDomainEventProjectionGrainWithGuidKey.

[ImplicitStreamSubscription(TaskConstants.TaskEvent.Base)]
public class TaskViewDailyStatsRouterGrain : DomainEventRouterGrain<TaskView.DailyStats>, IGrainWithGuidKey
{
  protected override object GetTargetKey(DomainEvent domainEvent)
  {
    return TaskView.DailyStats.GetKey(domainEvent.Metadata);
  }
}

public class TaskViewDailyStatsUpdateGrain
  : EntityFrameworkCoreUpdateProjectionGrain<ToDoDbContext, TaskView.DailyStats, TaskEvent>,
    IUpdateDomainEventProjectionGrainWithStringKey<TaskView.DailyStats>
{
  public TaskViewDailyStatsUpdateGrain()
    : base(TaskConstants.TaskView.DailyStats) { }
}
