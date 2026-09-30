using Flensrocker.EventSourcing.Abstractions;
using Flensrocker.EventSourcing.Orleans.Abstractions;
using Flensrocker.EventSourcing.Orleans.EntityFrameworkCore;
using Flensrocker.ToDo.Abstractions.Tasks;
using Flensrocker.ToDo.Persistence.PostgreSQL;
using Microsoft.Extensions.Logging;
using Orleans.Streams;
using Orleans.Streams.Core;

namespace Flensrocker.ToDo.Orleans.Server.Tasks;

[ImplicitStreamSubscription(TaskConstants.TaskEvent.Base)]
public class TaskViewDailyStatsRouterGrain : Grain, IGrainWithGuidKey, IStreamSubscriptionObserver, IAsyncObserver<DomainEvent>
{
  private readonly ILogger<TaskViewDailyStatsRouterGrain> _logger;

  public TaskViewDailyStatsRouterGrain(ILogger<TaskViewDailyStatsRouterGrain> logger)
  {
    _logger = logger;
  }

  // TODO extract stream handling to base class, e.g. DomainEventRouterGrain => how to handle Guid/string keys?
  public async Task OnSubscribed(IStreamSubscriptionHandleFactory handleFactory)
  {
    var handle = handleFactory.Create<DomainEvent>();
    await handle.ResumeAsync(this);
  }

  public async Task OnNextAsync(DomainEvent domainEvent, StreamSequenceToken? token = null)
  {
    var dailyStatsId = TaskView.DailyStats.GetKey(domainEvent.Metadata);

    var dailyStatsGrain = GrainFactory.GetGrain<IUpdateDomainEventProjectionGrainWithStringKey<TaskView.DailyStats>>(dailyStatsId);
    await dailyStatsGrain.UpdateProjection(domainEvent);
  }

  public Task OnErrorAsync(Exception ex)
  {
    // TODO add error handling
    _logger.LogError(ex, "Error from Orleans Stream");

    return Task.CompletedTask;
  }
}

public class TaskViewDailyStatsUpdateGrain
  : EntityFrameworkCoreUpdateProjectionGrain<ToDoDbContext, TaskView.DailyStats, TaskEvent>,
    IUpdateDomainEventProjectionGrainWithStringKey<TaskView.DailyStats>
{
  public TaskViewDailyStatsUpdateGrain()
    : base(TaskConstants.TaskView.DailyStats) { }
}
