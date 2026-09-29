namespace Flensrocker.EventSourcing.Persistence.PostgreSQL;

internal static class PostgreSQLPersistenceConstants
{
  internal const string PublicationName = "eventsourcingmessagepub";
  internal const string PublicationSlotName = PublicationName + "slot";
}
