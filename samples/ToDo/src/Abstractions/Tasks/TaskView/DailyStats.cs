using System.Linq.Expressions;
using System.Text.Json.Serialization;
using Flensrocker.EventSourcing.Abstractions;

namespace Flensrocker.ToDo.Abstractions.Tasks;

public abstract partial record TaskView
{
  [GenerateSerializer]
  [Alias(TaskConstants.TaskView.DailyStats)]
  public sealed record DailyStats : TaskView, ITaskEventProjectable, IWhereKey<DailyStats>
  {
    public const int ProjectionVersion = 1;

    public static (DateOnly Day, Guid TenantId, Guid UserId) GetKey(DomainEventMetadata metadata)
    {
      // for simplicity timezones don't matter in this statistic
      (var day, _, _) = metadata.CreatedAt;
      // TenantId and UserId will be part of the primary key and therefore cannot be null
      // This is a bit leaking implementation details from the infrastructure into the domain
      // TODO TECHNICAL DEBT
      return (day, metadata.TenantId ?? Guid.Empty, metadata.CreatedByUserId ?? Guid.Empty);
    }

    static Expression<Func<DailyStats, bool>> IWhereKey<DailyStats>.GetWhereKey(DomainEventMetadata metadata)
    {
      var (day, tenantId, userId) = GetKey(metadata);
      return item => item.Day == day && item.TenantId == tenantId && item.UserId == userId;
    }

    [Id(0)]
    [JsonInclude]
    public DateOnly Day { get; internal set; }

    [Id(1)]
    [JsonInclude]
    public Guid TenantId { get; internal set; }

    [Id(2)]
    [JsonInclude]
    public Guid UserId { get; internal set; }

    [Id(3)]
    [JsonInclude]
    public int CountNoted { get; internal set; }

    [Id(4)]
    [JsonInclude]
    public int CountCompleted { get; internal set; }

    [Id(5)]
    [JsonInclude]
    public int CountDismissed { get; internal set; }

    void ITaskEventProjectable.Apply(DomainEventMetadata metadata, TaskEvent.Completed payload)
    {
      CountCompleted++;
    }

    void ITaskEventProjectable.Apply(DomainEventMetadata metadata, TaskEvent.Dismissed payload)
    {
      CountDismissed++;
    }

    void ITaskEventProjectable.Apply(DomainEventMetadata metadata, TaskEvent.Noted payload)
    {
      CountNoted++;
    }

    void ITaskEventProjectable.Apply(DomainEventMetadata metadata, TaskEvent.Reopened payload)
    {
      // for simplicity a reopening of a task is like noting a new task
      CountNoted++;
    }

    public void Apply(DomainEvent @event)
    {
      (Day, TenantId, UserId) = GetKey(@event.Metadata);

      ((ITaskEventProjectable)this).ApplyTaskEvent(@event);
    }
  }
}
