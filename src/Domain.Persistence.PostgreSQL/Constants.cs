namespace Flensrocker.Domain.Persistence.PostgreSQL;

internal static class Constants
{
  internal const string SchemaName = "Domain";
  internal const string MessageTableName = "Message";
  internal const string GlobalMessageIndexSequenceName = "GlobalMessageIndex";
  internal const string PublicationName = "domain_message_pub";
  internal const string PublicationSlotName = PublicationName + "_slot";
  internal const int MaxLengthStreamType = 250;
}
