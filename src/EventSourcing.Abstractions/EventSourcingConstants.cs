namespace Flensrocker.EventSourcing.Abstractions;

public static class EventSourcingConstants
{
  public const string TraceSourceName = $"{nameof(Flensrocker)}.{nameof(EventSourcing)}";
  public const string TraceVersion = "1.0.0";

  public const string MeterName = $"{nameof(Flensrocker)}.{nameof(EventSourcing)}";
  public const string MeterVersion = "1.0.0";

  public const string AliasBase = "Flensrocker_EventSourcing";

  internal static class DomainResult
  {
    internal const string Base = $"{AliasBase}_DomainResult";
    internal const string BadRequest = $"{Base}_BadRequest";
    internal const string Forbid = $"{Base}_Forbid";
    internal const string InternalError = $"{Base}_InternalError";
    internal const string NotFound = $"{Base}_NotFound";
    internal const string Success = $"{Base}_Success";
    internal const string ValidationProblem = $"{Base}_ValidationProblem";
    internal const string Unauthorized = $"{Base}_Unauthorized";
  }

  internal static class ExecutionResult
  {
    internal const string Base = $"{AliasBase}_ExecutionResult";
  }
}
