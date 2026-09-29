namespace Flensrocker.EventSourcing.Persistence.EntityFrameworkCore;

public static class EntityFrameworkCoreConstants
{
  public const string SchemaName = "EventSourcing";
  public const string MessageTableName = "Message";
  public const string GlobalMessageIndexSequenceName = "GlobalMessageIndex";
  public const int MaxLengthStreamType = 250;
}
