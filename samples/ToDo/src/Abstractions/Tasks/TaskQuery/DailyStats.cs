using System.Text.Json.Serialization;

namespace Flensrocker.ToDo.Abstractions.Tasks;

public abstract partial record TaskQueryRequest
{
  [GenerateSerializer]
  [Alias(TaskConstants.TaskQueryRequest.DailyStats)]
  public sealed record DailyStats : TaskQueryRequest
  {
    [Id(0)]
    public Guid? TenantId { get; init; }

    [Id(1)]
    public Guid? UserId { get; init; }

    [Id(2)]
    public DateOnly? FromDate { get; init; }

    [Id(3)]
    public DateOnly? ToDate { get; init; }

    [JsonConstructor]
    internal DailyStats() { }

    private const string _errorTitle = "Invalid 'DailyStats' query";
    private const string _errorDetail = "The query cannot be executed.";

    public static DomainResult<DailyStats> Create(Guid? tenantId, Guid? userId, DateOnly? fromDate, DateOnly? toDate)
    {
      if (fromDate.HasValue && toDate.HasValue && fromDate.Value > toDate.Value)
      {
        return new DomainResult.ValidationProblem(
          _errorTitle,
          _errorDetail,
          nameof(FromDate),
          "\"From Date\" must be before \"To Date\""
        ).AddError(nameof(ToDate), "\"To Date\" must be after or equal to \"From Date\"");
      }

      return new DailyStats
      {
        TenantId = tenantId,
        UserId = userId,
        FromDate = fromDate,
        ToDate = toDate,
      };
    }
  }
}

public abstract partial record TaskQueryResponse
{
  [GenerateSerializer]
  [Alias(TaskConstants.TaskQueryResponse.DailyStats)]
  public sealed record DailyStats : TaskQueryResponse
  {
    [GenerateSerializer]
    [Alias(TaskConstants.TaskQueryResponse.DailyStats_Item)]
    public sealed record DailyStatsItem
    {
      [Id(0)]
      public required DateOnly Day { get; init; }

      [Id(1)]
      public required Guid? UserId { get; init; }

      [Id(2)]
      public required int CountNoted { get; init; }

      [Id(3)]
      public required int CountCompleted { get; init; }

      [Id(4)]
      public required int CountDismissed { get; init; }
    }

    [Id(0)]
    public required IReadOnlyCollection<DailyStatsItem> ItemList { get; init; }
  }
}
