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

    private const string _nullIdKeyPart = "________________________________";

    private static string ToKeyPart(Guid? id)
    {
      return id.HasValue ? id.Value.ToString("N") : _nullIdKeyPart;
    }

    private static string ToKeyPart(DateOnly day)
    {
      return day.ToString("yyyyMMdd");
    }

    public static string GetKey(DomainEvent domainEvent)
    {
      // for simplicity timezones don't matter in this statistic
      (var day, _, _) = domainEvent.Metadata.CreatedAt;
      return $"{ToKeyPart(day)}{ToKeyPart(domainEvent.Metadata.TenantId)}{ToKeyPart(domainEvent.Metadata.CreatedByUserId)}";
    }

    static object IWhereKey<DailyStats>.GetKey(DomainEvent domainEvent) => GetKey(domainEvent);

    static Expression<Func<DailyStats, bool>> IWhereKey<DailyStats>.GetWhereKey(DomainEvent domainEvent)
    {
      var dailyStatsId = GetKey(domainEvent);
      return item => item.DailyStatsId == dailyStatsId;
    }

    [Id(0)]
    [JsonInclude]
    public string DailyStatsId { get; internal set; } = "";

    [Id(1)]
    [JsonInclude]
    public DateOnly Day { get; internal set; }

    [Id(2)]
    [JsonInclude]
    public Guid? TenantId { get; internal set; }

    [Id(3)]
    [JsonInclude]
    public Guid? UserId { get; internal set; }

    [Id(4)]
    [JsonInclude]
    public int CountOpened { get; internal set; }

    [Id(5)]
    [JsonInclude]
    public int CountCompleted { get; internal set; }

    [Id(6)]
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

    void ITaskEventProjectable.Apply(DomainEventMetadata metadata, TaskEvent.Opened payload)
    {
      CountOpened++;
    }

    void ITaskEventProjectable.Apply(DomainEventMetadata metadata, TaskEvent.Paused payload) { }

    void ITaskEventProjectable.Apply(DomainEventMetadata metadata, TaskEvent.Reopened payload)
    {
      // for simplicity a reopening of a task is like opening a new task
      CountOpened++;
    }

    void ITaskEventProjectable.Apply(DomainEventMetadata metadata, TaskEvent.Started payload) { }

    void ITaskEventProjectable.Apply(DomainEventMetadata metadata, TaskEvent.TitleChanged payload) { }

    public void Apply(DomainEvent @event)
    {
      DailyStatsId = GetKey(@event);
      (Day, _, _) = @event.Metadata.CreatedAt;
      TenantId = @event.Metadata.TenantId;
      UserId = @event.Metadata.CreatedByUserId;

      ((ITaskEventProjectable)this).ApplyTaskEvent(@event);
    }
  }
}
