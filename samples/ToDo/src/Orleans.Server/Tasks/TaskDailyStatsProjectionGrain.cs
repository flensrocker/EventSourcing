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
public class TaskDailyStatsProjectionRouterGrain : Grain, IGrainWithGuidKey, IStreamSubscriptionObserver, IAsyncObserver<DomainEvent>
{
  private readonly ILogger<TaskDailyStatsProjectionRouterGrain> _logger;
  private IStreamProvider _streamProvider = default!;

  public TaskDailyStatsProjectionRouterGrain(ILogger<TaskDailyStatsProjectionRouterGrain> logger)
  {
    _logger = logger;
  }

  public override Task OnActivateAsync(CancellationToken cancellationToken)
  {
    _streamProvider = this.GetStreamProvider(OrleansConstants.DomainEventStreamProviderName);

    return Task.CompletedTask;
  }

  public async Task OnSubscribed(IStreamSubscriptionHandleFactory handleFactory)
  {
    var handle = handleFactory.Create<DomainEvent>();
    await handle.ResumeAsync(this);
  }

  public async Task OnNextAsync(DomainEvent domainEvent, StreamSequenceToken? token = null)
  {
    var dailyStatsId = TaskView.DailyStats.GetKey(domainEvent.Metadata);

    var streamId = StreamId.Create(TaskConstants.TaskView.DailyStats, dailyStatsId);
    var stream = _streamProvider.GetStream<DomainEvent>(streamId);
    await stream.OnNextAsync(domainEvent);
  }

  public Task OnErrorAsync(Exception ex)
  {
    // TODO add error handling
    _logger.LogError(ex, "Error from Orleans Stream");

    return Task.CompletedTask;
  }
}

[ImplicitStreamSubscription(TaskConstants.TaskView.DailyStats)]
public class TaskDailyStatsProjectionGrain
  : EntityFrameworkCoreProjectionGrain<ToDoDbContext, TaskView.DailyStats, TaskEvent>,
    IGrainWithStringKey
{
  public TaskDailyStatsProjectionGrain()
    : base(TaskConstants.TaskView.DailyStats) { }
}
