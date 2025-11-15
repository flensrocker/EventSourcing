namespace Flensrocker.EventSourcing.Persistence.PostgreSQL;

internal static class Constants
{
  internal const string SchemaName = "EventSourcing";
  internal const string MessageTableName = "Message";
  internal const string GlobalMessageIndexSequenceName = "GlobalMessageIndex";
  internal const string PublicationName = "eventsourcingmessagepub";
  internal const string PublicationSlotName = PublicationName + "slot";
  internal const int MaxLengthStreamType = 250;
}
